namespace QuizService.Model;

public class AnswerCreateModel
{
    public AnswerCreateModel(string text)
    {
        Text = text;
    }

    //TODO: I would add model validation controlls. [Required], [StringLength]
    public string Text { get; set; }
}