const api = "/api/tests";

const state = {
  tests: [],
  editingId: null,
  draft: emptyDraft(),
  takingTest: null,
  selectedAnswers: {}
};

const elements = {
  status: document.querySelector("#status"),
  list: document.querySelector("#listView"),
  editor: document.querySelector("#editorView"),
  details: document.querySelector("#detailsView"),
  take: document.querySelector("#takeView"),
  form: document.querySelector("#testForm"),
  title: document.querySelector("#titleInput"),
  description: document.querySelector("#descriptionInput"),
  questionsEditor: document.querySelector("#questionsEditor")
};

document.querySelector("#refreshButton").addEventListener("click", loadTests);
document.querySelector("#newButton").addEventListener("click", createTest);
document.querySelector("#addQuestionButton").addEventListener("click", addQuestion);
elements.title.addEventListener("input", () => {
  state.draft.title = elements.title.value;
});
elements.description.addEventListener("input", () => {
  state.draft.description = elements.description.value;
});
elements.form.addEventListener("submit", saveDraft);

loadTests();

function emptyDraft() {
  return {
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
}

async function request(url, options = {}) {
  const response = await fetch(url, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...(options.headers || {})
    }
  });

  if (!response.ok) {
    const problem = await response.json().catch(() => ({}));
    throw new Error(problem.errors?.join("\n") || problem.detail || `HTTP ${response.status}`);
  }

  return response.status === 204 ? null : response.json();
}

async function run(action) {
  elements.status.textContent = "Загрузка...";

  try {
    await action();
    elements.status.textContent = "";
  } catch (error) {
    elements.status.textContent = error.message || "Ошибка";
  }
}

function show(view) {
  for (const element of [elements.list, elements.editor, elements.details, elements.take]) {
    element.classList.add("hidden");
  }

  view.classList.remove("hidden");
}

async function loadTests() {
  await run(async () => {
    state.tests = await request(api);
    renderList();
    show(elements.list);
  });
}

function renderList() {
  elements.list.innerHTML = state.tests
    .map(
      (test) => `
        <article class="card">
          <div>
            <h2>${escapeHtml(test.title)}</h2>
            <p>${escapeHtml(test.description || "Без описания")}</p>
            <p><strong>${test.questionCount}</strong> вопросов</p>
          </div>
          <div class="actions">
            <button class="ghost" onclick="openDetails('${test.id}')">Открыть</button>
            <button class="ghost" onclick="editTest('${test.id}')">Править</button>
            <button onclick="startTaking('${test.id}')">Пройти</button>
          </div>
        </article>
      `
    )
    .join("");
}

function createTest() {
  state.editingId = null;
  state.draft = emptyDraft();
  renderEditor();
  show(elements.editor);
}

async function openDetails(id) {
  await run(async () => {
    const test = await request(`${api}/${id}`);
    elements.details.innerHTML = `
      <div class="actions">
        <button onclick="startTaking('${test.id}')">Пройти</button>
        <button class="ghost" onclick="editTest('${test.id}')">Редактировать</button>
        <button class="danger" onclick="deleteTest('${test.id}')">Удалить</button>
      </div>
      <section class="panel">
        <h2>${escapeHtml(test.title)}</h2>
        <p>${escapeHtml(test.description || "Без описания")}</p>
      </section>
      ${test.questions.map(renderQuestionDetails).join("")}
    `;
    show(elements.details);
  });
}

function renderQuestionDetails(question, index) {
  return `
    <article class="question">
      <div class="question-head">
        <span>Вопрос ${index + 1}</span>
        <strong>${question.type === "SingleChoice" ? "Один ответ" : "Несколько ответов"}</strong>
      </div>
      <h3>${escapeHtml(question.text)}</h3>
      <ul class="choice-list">
        ${question.options
          .map((option) => `<li class="${option.isCorrect ? "correct" : ""}">${escapeHtml(option.text)}</li>`)
          .join("")}
      </ul>
    </article>
  `;
}

async function editTest(id) {
  await run(async () => {
    const test = await request(`${api}/${id}`);
    state.editingId = id;
    state.draft = {
      title: test.title,
      description: test.description || "",
      questions: test.questions.map((question) => ({
        text: question.text,
        type: question.type,
        options: question.options.map((option) => ({
          text: option.text,
          isCorrect: option.isCorrect
        }))
      }))
    };
    renderEditor();
    show(elements.editor);
  });
}

function renderEditor() {
  elements.title.value = state.draft.title;
  elements.description.value = state.draft.description || "";
  elements.questionsEditor.innerHTML = state.draft.questions.map(renderQuestionEditor).join("");
}

function renderQuestionEditor(question, questionIndex) {
  return `
    <article class="question">
      <div class="question-head">
        <span>Вопрос ${questionIndex + 1}</span>
        <button class="danger" type="button" onclick="removeQuestion(${questionIndex})">Удалить</button>
      </div>
      <label>
        Текст вопроса
        <input required value="${escapeAttribute(question.text)}" oninput="setQuestionText(${questionIndex}, this.value)" />
      </label>
      <label>
        Тип
        <select onchange="setQuestionType(${questionIndex}, this.value)">
          <option value="SingleChoice" ${question.type === "SingleChoice" ? "selected" : ""}>Один ответ</option>
          <option value="MultipleChoice" ${question.type === "MultipleChoice" ? "selected" : ""}>Несколько ответов</option>
        </select>
      </label>
      ${question.options.map((option, optionIndex) => renderOptionEditor(question, questionIndex, option, optionIndex)).join("")}
      <button class="ghost" type="button" onclick="addOption(${questionIndex})">Добавить ответ</button>
    </article>
  `;
}

function renderOptionEditor(question, questionIndex, option, optionIndex) {
  return `
    <div class="option-row">
      <input required value="${escapeAttribute(option.text)}" placeholder="Ответ ${optionIndex + 1}" oninput="setOptionText(${questionIndex}, ${optionIndex}, this.value)" />
      <label class="correct-toggle">
        <input
          type="${question.type === "SingleChoice" ? "radio" : "checkbox"}"
          name="correct-${questionIndex}"
          ${option.isCorrect ? "checked" : ""}
          onchange="toggleCorrect(${questionIndex}, ${optionIndex})"
        />
        Верный
      </label>
      <button class="ghost" type="button" onclick="removeOption(${questionIndex}, ${optionIndex})" ${question.options.length <= 2 ? "disabled" : ""}>Убрать</button>
    </div>
  `;
}

async function saveDraft(event) {
  event.preventDefault();
  state.draft.title = elements.title.value;
  state.draft.description = elements.description.value;

  await run(async () => {
    const saved = state.editingId
      ? await request(`${api}/${state.editingId}`, { method: "PUT", body: JSON.stringify(state.draft) })
      : await request(api, { method: "POST", body: JSON.stringify(state.draft) });

    await loadTests();
    await openDetails(saved.id);
  });
}

async function deleteTest(id) {
  if (!confirm("Удалить тест?")) {
    return;
  }

  await run(async () => {
    await request(`${api}/${id}`, { method: "DELETE" });
    await loadTests();
  });
}

async function startTaking(id) {
  await run(async () => {
    state.takingTest = await request(`${api}/${id}/take`);
    state.selectedAnswers = {};
    renderTaking();
    show(elements.take);
  });
}

function renderTaking(result = null) {
  const test = state.takingTest;
  elements.take.innerHTML = `
    <section class="panel">
      <h2>${escapeHtml(test.title)}</h2>
      <p>${escapeHtml(test.description || "Без описания")}</p>
    </section>
    ${test.questions.map(renderTakeQuestion).join("")}
    <button onclick="submitAnswers()">Завершить</button>
    ${result ? `<section class="result"><span>${result.score} / ${result.maxScore}</span><span>${result.percentage}%</span></section>` : ""}
  `;
}

function renderTakeQuestion(question, index) {
  const selected = state.selectedAnswers[question.id] || [];
  return `
    <article class="question">
      <div class="question-head">
        <span>Вопрос ${index + 1}</span>
        <strong>${question.type === "SingleChoice" ? "Один ответ" : "Несколько ответов"}</strong>
      </div>
      <h3>${escapeHtml(question.text)}</h3>
      <div class="stack">
        ${question.options
          .map(
            (option) => `
              <label class="take-option">
                <input
                  type="${question.type === "SingleChoice" ? "radio" : "checkbox"}"
                  name="answer-${question.id}"
                  ${selected.includes(option.id) ? "checked" : ""}
                  onchange="toggleAnswer('${question.id}', '${option.id}', '${question.type}')"
                />
                ${escapeHtml(option.text)}
              </label>
            `
          )
          .join("")}
      </div>
    </article>
  `;
}

async function submitAnswers() {
  await run(async () => {
    const result = await request(`${api}/${state.takingTest.id}/submissions`, {
      method: "POST",
      body: JSON.stringify({
        answers: state.takingTest.questions.map((question) => ({
          questionId: question.id,
          selectedOptionIds: state.selectedAnswers[question.id] || []
        }))
      })
    });
    renderTaking(result);
    show(elements.take);
  });
}

function setQuestionText(index, text) {
  state.draft.questions[index].text = text;
}

function setQuestionType(index, type) {
  const question = state.draft.questions[index];
  question.type = type;

  if (type === "SingleChoice") {
    let found = false;
    for (const option of question.options) {
      option.isCorrect = option.isCorrect && !found;
      found ||= option.isCorrect;
    }

    if (!found) {
      question.options[0].isCorrect = true;
    }
  }

  renderEditor();
}

function setOptionText(questionIndex, optionIndex, text) {
  state.draft.questions[questionIndex].options[optionIndex].text = text;
}

function toggleCorrect(questionIndex, optionIndex) {
  const question = state.draft.questions[questionIndex];

  if (question.type === "SingleChoice") {
    question.options.forEach((option, index) => {
      option.isCorrect = index === optionIndex;
    });
  } else {
    question.options[optionIndex].isCorrect = !question.options[optionIndex].isCorrect;
  }

  renderEditor();
}

function addQuestion() {
  state.draft.questions.push(emptyDraft().questions[0]);
  renderEditor();
}

function removeQuestion(index) {
  state.draft.questions.splice(index, 1);
  renderEditor();
}

function addOption(questionIndex) {
  state.draft.questions[questionIndex].options.push({ text: "", isCorrect: false });
  renderEditor();
}

function removeOption(questionIndex, optionIndex) {
  state.draft.questions[questionIndex].options.splice(optionIndex, 1);
  renderEditor();
}

function toggleAnswer(questionId, optionId, type) {
  const selected = state.selectedAnswers[questionId] || [];
  state.selectedAnswers[questionId] =
    type === "SingleChoice"
      ? [optionId]
      : selected.includes(optionId)
        ? selected.filter((id) => id !== optionId)
        : [...selected, optionId];
}

function escapeHtml(value) {
  return String(value)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#039;");
}

function escapeAttribute(value) {
  return escapeHtml(value).replaceAll("\n", " ");
}

Object.assign(window, {
  openDetails,
  editTest,
  deleteTest,
  startTaking,
  setQuestionText,
  setQuestionType,
  setOptionText,
  toggleCorrect,
  addOption,
  removeOption,
  removeQuestion,
  toggleAnswer,
  submitAnswers
});
