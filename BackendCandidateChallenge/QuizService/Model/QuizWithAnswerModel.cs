namespace QuizService.Model {
    public class QuizWithAnswerModel {
        public int Id { get; set; }

        public string Title { get; set; }

        public int? QuestionId { get; set; }

        public string Text { get; set; }
        
        public int CorrectAnswerId { get; set; }

        public int? AnswerId { get; set; }
        
        public string AnswerText { get; set; }

    }
}
