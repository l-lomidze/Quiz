using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

public class QuizContext : DbContext {
    public DbSet<Student> Students { get; set; }
    public DbSet<Subject> Subjects { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options) {
        options.UseSqlite("Data source=Quiz.db");
        base.OnConfiguring(options);
    }
}

public class Student {
    [Key]
    public int StudentId { get; set; }
    public string Name { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public List<Subject> EnrolledSubjects { get; set; } = new();
}

public class Subject {

    [Key]
    public int SubjectId { get; set; }
    public string Title { get; set; }
    public int MaximumCapacity { get; set; }
    public List<Student> EnrolledStudents { get; set; } = new();
}

public class QuizRepository {
    private QuizContext dbContext;

    public QuizRepository() {
        this.dbContext = new QuizContext();
        dbContext.Database.EnsureCreated();
    }

    public int AddSubject(Subject subject) {
        dbContext.Add(subject);
        dbContext.SaveChanges();
        return subject.SubjectId;
    }

    public int AddStudent(Student student) {
        dbContext.Add(student);
        dbContext.SaveChanges();
        return student.StudentId;
    }

    public void EnrollStudentToSubject(int studentId, int subjectId) {
        var subject = dbContext.Subjects
            .Include(subject => subject.EnrolledStudents)
            .FirstOrDefault(subject => subject.SubjectId == subjectId);
        
        var student = dbContext.Students
                .FirstOrDefault(student => student.StudentId == studentId);

        subject?.EnrolledStudents.Add(student);

        dbContext.SaveChanges();
    }

    public List<Subject> GetAllSubjects() {
        return dbContext.Subjects.ToList();
    }

    public List<Student>? GetStudentsForSubject(int subjectId) {
        return dbContext.Subjects
            .Include(subject => subject.EnrolledStudents)
            .FirstOrDefault(subject => subject.SubjectId == subjectId)
            ?.EnrolledStudents
            ?.ToList();
    }

}

public class Program {
    private static QuizRepository repository = new();

    public static void Main() {
        var subject1 = new Subject() { Title = ".NET", MaximumCapacity = 20 };

        var student1 = new Student() { Name = "John", EnrollmentDate = DateTime.Now };
        var student2 = new Student() { Name = "Jane", EnrollmentDate = DateTime.Now };

        repository.AddSubject(subject1);

        repository.AddStudent(student1);
        repository.AddStudent(student2);

        repository.EnrollStudentToSubject(student1.StudentId, subject1.SubjectId);
        repository.EnrollStudentToSubject(student2.StudentId, subject1.SubjectId);

        student1.EnrolledSubjects.ForEach(subject => Console.WriteLine(subject.Title));

        subject1.EnrolledStudents.ForEach(student => Console.WriteLine(student.Name));
    }
}
