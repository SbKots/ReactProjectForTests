export type QuestionType = "SingleChoice" | "MultipleChoice";

export interface TestSummary {
  id: string;
  title: string;
  description?: string | null;
  questionCount: number;
  updatedAtUtc: string;
}

export interface TestDetails {
  id: string;
  title: string;
  description?: string | null;
  questions: QuestionDetails[];
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface QuestionDetails {
  id: string;
  text: string;
  type: QuestionType;
  options: AnswerOptionDetails[];
}

export interface AnswerOptionDetails {
  id: string;
  text: string;
  isCorrect: boolean;
}

export interface TestForTaking {
  id: string;
  title: string;
  description?: string | null;
  questions: QuestionForTaking[];
}

export interface QuestionForTaking {
  id: string;
  text: string;
  type: QuestionType;
  options: AnswerOptionForTaking[];
}

export interface AnswerOptionForTaking {
  id: string;
  text: string;
}

export interface TestWriteRequest {
  title: string;
  description?: string | null;
  questions: QuestionWriteDto[];
}

export interface QuestionWriteDto {
  text: string;
  type: QuestionType;
  options: AnswerOptionWriteDto[];
}

export interface AnswerOptionWriteDto {
  text: string;
  isCorrect: boolean;
}

export interface SubmitTestRequest {
  answers: QuestionAnswerSubmission[];
}

export interface QuestionAnswerSubmission {
  questionId: string;
  selectedOptionIds: string[];
}

export interface TestSubmissionResult {
  testId: string;
  score: number;
  maxScore: number;
  percentage: number;
  questions: QuestionResult[];
}

export interface QuestionResult {
  questionId: string;
  score: number;
  maxScore: number;
  correctOptionIds: string[];
  selectedOptionIds: string[];
}
