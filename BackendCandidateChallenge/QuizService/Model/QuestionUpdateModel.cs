namespace QuizService.Model;

public class QuestionUpdateModel
{
    //TODO: I would add model validation controlls. [Required], [StringLength]
    public string Text { get; set; }
    public int CorrectAnswerId { get; set; }
}