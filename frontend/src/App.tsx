import { FormEvent, useEffect, useMemo, useState } from "react";
import { testsApi } from "./api/testsApi";
import type {
  QuestionType,
  TestDetails,
  TestForTaking,
  TestSubmissionResult,
  TestSummary,
  TestWriteRequest
} from "./types";

type Screen = "list" | "details" | "editor" | "take";

const emptyDraft: TestWriteRequest = {
  title: "",
  description: "",
  questions: [
    {
      text: "",
      type: "SingleChoice",
      options: [
        { text: "", isCorrect: true },
        { text: "", isCorrect: false }
      ]
    }
  ]
};

export default function App() {
  const [screen, setScreen] = useState<Screen>("list");
  const [tests, setTests] = useState<TestSummary[]>([]);
  const [selectedTest, setSelectedTest] = useState<TestDetails | null>(null);
  const [takingTest, setTakingTest] = useState<TestForTaking | null>(null);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [draft, setDraft] = useState<TestWriteRequest>(emptyDraft);
  const [selectedAnswers, setSelectedAnswers] = useState<Record<string, string[]>>({});
  const [result, setResult] = useState<TestSubmissionResult | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    void loadTests();
  }, []);

  const headerTitle = useMemo(() => {
    if (screen === "editor") {
      return editingId ? "Редактирование теста" : "Новый тест";
    }

    if (screen === "take") {
      return takingTest?.title ?? "Прохождение";
    }

    if (screen === "details") {
      return selectedTest?.title ?? "Просмотр";
    }

    return "Система управления тестами";
  }, [editingId, screen, selectedTest, takingTest]);

  async function run(action: () => Promise<void>) {
    setLoading(true);
    setError(null);

    try {
      await action();
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : "Неизвестная ошибка");
    } finally {
      setLoading(false);
    }
  }

  async function loadTests() {
    await run(async () => {
      const loadedTests = await testsApi.list();
      setTests(loadedTests);
    });
  }

  async function openDetails(id: string) {
    await run(async () => {
      setSelectedTest(await testsApi.get(id));
      setScreen("details");
    });
  }

  async function startTaking(id: string) {
    await run(async () => {
      const test = await testsApi.getForTaking(id);
      setTakingTest(test);
      setSelectedAnswers({});
      setResult(null);
      setScreen("take");
    });
  }

  async function editTest(id: string) {
    await run(async () => {
      const test = await testsApi.get(id);
      setEditingId(id);
      setDraft({
        title: test.title,
        description: test.description ?? "",
        questions: test.questions.map((question) => ({
          text: question.text,
          type: question.type,
          options: question.options.map((option) => ({
            text: option.text,
            isCorrect: option.isCorrect
          }))
        }))
      });
      setScreen("editor");
    });
  }

  function createTest() {
    setEditingId(null);
    setDraft(structuredClone(emptyDraft));
    setScreen("editor");
  }

  async function deleteTest(id: string) {
    if (!confirm("Удалить тест?")) {
      return;
    }

    await run(async () => {
      await testsApi.remove(id);
      await loadTests();
      setScreen("list");
    });
  }

  async function saveDraft(event: FormEvent) {
    event.preventDefault();

    await run(async () => {
      const saved = editingId
        ? await testsApi.update(editingId, draft)
        : await testsApi.create(draft);

      await loadTests();
      setSelectedTest(saved);
      setScreen("details");
    });
  }

  async function submitAnswers() {
    if (!takingTest) {
      return;
    }

    await run(async () => {
      const payload = {
        answers: takingTest.questions.map((question) => ({
          questionId: question.id,
          selectedOptionIds: selectedAnswers[question.id] ?? []
        }))
      };

      setResult(await testsApi.submit(takingTest.id, payload));
    });
  }

  function updateQuestion(index: number, patch: Partial<TestWriteRequest["questions"][number]>) {
    setDraft((current) => ({
      ...current,
      questions: current.questions.map((question, questionIndex) =>
        questionIndex === index ? { ...question, ...patch } : question
      )
    }));
  }

  function changeQuestionType(index: number, type: QuestionType) {
    setDraft((current) => ({
      ...current,
      questions: current.questions.map((question, questionIndex) => {
        if (questionIndex !== index) {
          return question;
        }

        if (type === "SingleChoice") {
          let firstCorrectUsed = false;
          return {
            ...question,
            type,
            options: question.options.map((option) => {
              if (!option.isCorrect || firstCorrectUsed) {
                return { ...option, isCorrect: false };
              }

              firstCorrectUsed = true;
              return option;
            })
          };
        }

        return { ...question, type };
      })
    }));
  }

  function updateOption(questionIndex: number, optionIndex: number, text: string) {
    setDraft((current) => ({
      ...current,
      questions: current.questions.map((question, currentQuestionIndex) =>
        currentQuestionIndex === questionIndex
          ? {
              ...question,
              options: question.options.map((option, currentOptionIndex) =>
                currentOptionIndex === optionIndex ? { ...option, text } : option
              )
            }
          : question
      )
    }));
  }

  function toggleCorrect(questionIndex: number, optionIndex: number) {
    setDraft((current) => ({
      ...current,
      questions: current.questions.map((question, currentQuestionIndex) => {
        if (currentQuestionIndex !== questionIndex) {
          return question;
        }

        return {
          ...question,
          options: question.options.map((option, currentOptionIndex) => {
            if (question.type === "SingleChoice") {
              return { ...option, isCorrect: currentOptionIndex === optionIndex };
            }

            return currentOptionIndex === optionIndex
              ? { ...option, isCorrect: !option.isCorrect }
              : option;
          })
        };
      })
    }));
  }

  function addQuestion() {
    setDraft((current) => ({
      ...current,
      questions: [
        ...current.questions,
        {
          text: "",
          type: "SingleChoice",
          options: [
            { text: "", isCorrect: true },
            { text: "", isCorrect: false }
          ]
        }
      ]
    }));
  }

  function removeQuestion(index: number) {
    setDraft((current) => ({
      ...current,
      questions: current.questions.filter((_, questionIndex) => questionIndex !== index)
    }));
  }

  function addOption(questionIndex: number) {
    setDraft((current) => ({
      ...current,
      questions: current.questions.map((question, currentQuestionIndex) =>
        currentQuestionIndex === questionIndex
          ? { ...question, options: [...question.options, { text: "", isCorrect: false }] }
          : question
      )
    }));
  }

  function removeOption(questionIndex: number, optionIndex: number) {
    setDraft((current) => ({
      ...current,
      questions: current.questions.map((question, currentQuestionIndex) =>
        currentQuestionIndex === questionIndex
          ? {
              ...question,
              options: question.options.filter((_, currentOptionIndex) => currentOptionIndex !== optionIndex)
            }
          : question
      )
    }));
  }

  function toggleSelected(questionId: string, optionId: string, type: QuestionType) {
    setSelectedAnswers((current) => {
      const selected = current[questionId] ?? [];

      if (type === "SingleChoice") {
        return { ...current, [questionId]: [optionId] };
      }

      return {
        ...current,
        [questionId]: selected.includes(optionId)
          ? selected.filter((id) => id !== optionId)
          : [...selected, optionId]
      };
    });
  }

  return (
    <main className="app-shell">
      <header className="topbar">
        <div>
          <p className="eyebrow">CRUD + прохождение</p>
          <h1>{headerTitle}</h1>
        </div>
        <nav className="actions">
          <button className="ghost" onClick={() => setScreen("list")}>
            Список
          </button>
          <button onClick={createTest}>Создать</button>
        </nav>
      </header>

      {error && <pre className="error">{error}</pre>}
      {loading && <div className="loading">Загрузка...</div>}

      {screen === "list" && (
        <section className="grid-list">
          {tests.map((test) => (
            <article className="test-card" key={test.id}>
              <div>
                <h2>{test.title}</h2>
                <p>{test.description || "Без описания"}</p>
                <span>{test.questionCount} вопросов</span>
              </div>
              <div className="card-actions">
                <button className="ghost" onClick={() => openDetails(test.id)}>
                  Открыть
                </button>
                <button className="ghost" onClick={() => editTest(test.id)}>
                  Править
                </button>
                <button onClick={() => startTaking(test.id)}>Пройти</button>
              </div>
            </article>
          ))}
        </section>
      )}

      {screen === "details" && selectedTest && (
        <section className="workspace">
          <div className="toolbar">
            <button onClick={() => startTaking(selectedTest.id)}>Пройти</button>
            <button className="ghost" onClick={() => editTest(selectedTest.id)}>
              Редактировать
            </button>
            <button className="danger" onClick={() => deleteTest(selectedTest.id)}>
              Удалить
            </button>
          </div>

          <p className="description">{selectedTest.description || "Без описания"}</p>
          {selectedTest.questions.map((question, index) => (
            <article className="question-panel" key={question.id}>
              <div className="question-heading">
                <span>Вопрос {index + 1}</span>
                <strong>{question.type === "SingleChoice" ? "Один ответ" : "Несколько ответов"}</strong>
              </div>
              <h3>{question.text}</h3>
              <ul className="option-list">
                {question.options.map((option) => (
                  <li className={option.isCorrect ? "correct" : ""} key={option.id}>
                    {option.text}
                  </li>
                ))}
              </ul>
            </article>
          ))}
        </section>
      )}

      {screen === "editor" && (
        <form className="workspace" onSubmit={saveDraft}>
          <div className="form-row">
            <label>
              Название
              <input
                maxLength={200}
                required
                value={draft.title}
                onChange={(event) => setDraft({ ...draft, title: event.target.value })}
              />
            </label>
            <label>
              Описание
              <textarea
                maxLength={4000}
                value={draft.description ?? ""}
                onChange={(event) => setDraft({ ...draft, description: event.target.value })}
              />
            </label>
          </div>

          {draft.questions.map((question, questionIndex) => (
            <article className="question-panel" key={questionIndex}>
              <div className="question-heading">
                <span>Вопрос {questionIndex + 1}</span>
                <button className="danger small" type="button" onClick={() => removeQuestion(questionIndex)}>
                  Удалить
                </button>
              </div>
              <label>
                Текст вопроса
                <input
                  required
                  value={question.text}
                  onChange={(event) => updateQuestion(questionIndex, { text: event.target.value })}
                />
              </label>
              <label>
                Тип
                <select
                  value={question.type}
                  onChange={(event) => changeQuestionType(questionIndex, event.target.value as QuestionType)}
                >
                  <option value="SingleChoice">Один ответ</option>
                  <option value="MultipleChoice">Несколько ответов</option>
                </select>
              </label>

              <div className="option-editor">
                {question.options.map((option, optionIndex) => (
                  <div className="option-row" key={optionIndex}>
                    <input
                      required
                      value={option.text}
                      placeholder={`Ответ ${optionIndex + 1}`}
                      onChange={(event) => updateOption(questionIndex, optionIndex, event.target.value)}
                    />
                    <label className="check-label">
                      <input
                        checked={option.isCorrect}
                        type={question.type === "SingleChoice" ? "radio" : "checkbox"}
                        onChange={() => toggleCorrect(questionIndex, optionIndex)}
                      />
                      Верный
                    </label>
                    <button
                      className="ghost small"
                      disabled={question.options.length <= 2}
                      type="button"
                      onClick={() => removeOption(questionIndex, optionIndex)}
                    >
                      Убрать
                    </button>
                  </div>
                ))}
                <button className="ghost" type="button" onClick={() => addOption(questionIndex)}>
                  Добавить ответ
                </button>
              </div>
            </article>
          ))}

          <div className="toolbar editor-actions">
            <button className="ghost" type="button" onClick={addQuestion}>
              Добавить вопрос
            </button>
            <button type="submit">Сохранить</button>
          </div>
        </form>
      )}

      {screen === "take" && takingTest && (
        <section className="workspace">
          <p className="description">{takingTest.description || "Без описания"}</p>

          {takingTest.questions.map((question, index) => (
            <article className="question-panel" key={question.id}>
              <div className="question-heading">
                <span>Вопрос {index + 1}</span>
                <strong>{question.type === "SingleChoice" ? "Один ответ" : "Несколько ответов"}</strong>
              </div>
              <h3>{question.text}</h3>
              <div className="take-options">
                {question.options.map((option) => (
                  <label key={option.id}>
                    <input
                      checked={(selectedAnswers[question.id] ?? []).includes(option.id)}
                      type={question.type === "SingleChoice" ? "radio" : "checkbox"}
                      onChange={() => toggleSelected(question.id, option.id, question.type)}
                    />
                    {option.text}
                  </label>
                ))}
              </div>
            </article>
          ))}

          <button onClick={submitAnswers}>Завершить</button>

          {result && (
            <section className="result-band">
              <strong>
                {result.score} / {result.maxScore}
              </strong>
              <span>{result.percentage}%</span>
            </section>
          )}
        </section>
      )}
    </main>
  );
}
