//Without explicit values, C# assigns 0, 1, 2 automatically.But if someone inserts a new role Any existing database it reads different data 
namespace Auth.API.Enums;
public enum UserRole
{
    User  = 0,
    Admin = 1,
    Agent = 2
}