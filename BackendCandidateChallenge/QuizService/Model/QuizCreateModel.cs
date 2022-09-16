namespace QuizService.Model;

public class QuizCreateModel
{
    public QuizCreateModel(string title)
    {
        Title = title;
    }

    //TODO: I would add model validation controlls. [Required], [StringLength]
    public string Title { get; set; }
}