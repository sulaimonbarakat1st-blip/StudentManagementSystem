namespace StudentManagementSystem.Models
{
    public class Student
    {
        
     public int Id { get; set; }

     public int MatricNo { get; set; }

     public required string FirstName { get; set; }

     public required string LastName { get; set; }

     public required string Email { get; set; }

     public DateTime DateOfBirth { get; set; }

     public required string Department { get; set; }

     public required string Faculty { get; set; }
  
    }
}

