namespace StudyMgt.Data.Entities;

public class StudentTutorAssignmentEntity
{
    public int Id { get; set; }
    public int StudentOnboardingId { get; set; }
    public int TutorOnboardingId { get; set; }
    public string TutorName { get; set; } = string.Empty;
    public DateTime AssignedDateUtc { get; set; } = DateTime.UtcNow;

    public StudentOnboardingEntity StudentOnboarding { get; set; } = null!;
}
