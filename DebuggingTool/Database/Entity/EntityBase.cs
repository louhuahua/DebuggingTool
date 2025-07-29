using SQLite;
using System;

namespace DebuggingTool.Database.Entity;

public class EntityBase
{
    [PrimaryKey,AutoIncrement]
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.Now;

}
