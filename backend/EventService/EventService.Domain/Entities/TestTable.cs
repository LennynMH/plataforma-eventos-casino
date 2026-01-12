namespace EventService.Domain.Entities;

/// <summary>
/// Tabla temporal para pruebas de migraciones
/// </summary>
public class TestTable
{
    public Guid Id { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
