namespace CantinaIBJ.Model.Core;

public interface IModel
{
    int Id { get; set; }

    bool IsDeleted { get; set; }
}