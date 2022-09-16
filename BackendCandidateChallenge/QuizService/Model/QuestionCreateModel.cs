namespace QuizService.Model;

public class QuestionCreateModel
{
    public QuestionCreateModel(string text)
    {
        Text = text;
    }

    //TODO: I would add model validation controlls. [Required], [StringLength]
    public string Text { get; set; }
}