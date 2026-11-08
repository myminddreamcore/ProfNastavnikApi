using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ProfNastavnik.Models;

public partial class ProfnastavnikContext : DbContext
{
    public ProfnastavnikContext()
    {
    }

    public ProfnastavnikContext(DbContextOptions<ProfnastavnikContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Adress> Adresses { get; set; }

    public virtual DbSet<Chat> Chats { get; set; }

    public virtual DbSet<CompanyPay> CompanyPays { get; set; }

    public virtual DbSet<CompanyPrice> CompanyPrices { get; set; }

    public virtual DbSet<CompanySfere> CompanySferes { get; set; }

    public virtual DbSet<Currency> Currencies { get; set; }

    public virtual DbSet<Deleting> Deletings { get; set; }

    public virtual DbSet<Director> Directors { get; set; }

    public virtual DbSet<Favourity> Favourities { get; set; }

    public virtual DbSet<FeedbacksCompany> FeedbacksCompanies { get; set; }

    public virtual DbSet<FeedbacksUser> FeedbacksUsers { get; set; }

    public virtual DbSet<Intership> Interships { get; set; }

    public virtual DbSet<MoneyType> MoneyTypes { get; set; }

    public virtual DbSet<PricesForCompany> PricesForCompanies { get; set; }

    public virtual DbSet<PricesForUser> PricesForUsers { get; set; }

    public virtual DbSet<Profession> Professions { get; set; }

    public virtual DbSet<RequestsCompany> RequestsCompanies { get; set; }

    public virtual DbSet<RequestsUser> RequestsUsers { get; set; }

    public virtual DbSet<Response> Responses { get; set; }

    public virtual DbSet<Sfere> Sferes { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentFile> StudentFiles { get; set; }

    public virtual DbSet<University> Universities { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserConsent> UserConsents { get; set; }

    public virtual DbSet<UserPay> UserPays { get; set; }

    public virtual DbSet<UserPrice> UserPrices { get; set; }

    public virtual DbSet<UserProfession> UserProfessions { get; set; }

    public virtual DbSet<UserSkill> UserSkills { get; set; }

    public virtual DbSet<VacanciesFormat> VacanciesFormats { get; set; }

    public virtual DbSet<Vacancy> Vacancies { get; set; }

    public virtual DbSet<VacancyFormat> VacancyFormats { get; set; }

    public virtual DbSet<VacancySfere> VacancySferes { get; set; }

    public virtual DbSet<VacancySkill> VacancySkills { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;port=5432;username=postgres;password=070507;database=profnastavnik;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Adress>(entity =>
        {
            entity.HasKey(e => e.IdAdress).HasName("adresses_pk");

            entity.ToTable("adresses", "profnastavnik");

            entity.Property(e => e.IdAdress)
                .ValueGeneratedNever()
                .HasColumnName("id_adress");
            entity.Property(e => e.CityAdress)
                .HasMaxLength(60)
                .HasColumnName("city_adress");
            entity.Property(e => e.FlatAdress)
                .HasMaxLength(6)
                .HasColumnName("flat_adress");
            entity.Property(e => e.HouseAdress)
                .HasMaxLength(6)
                .HasColumnName("house_adress");
            entity.Property(e => e.IdVacancy).HasColumnName("id_vacancy");
            entity.Property(e => e.IndexAdress).HasColumnName("index_adress");
            entity.Property(e => e.StreetAdress)
                .HasMaxLength(100)
                .HasColumnName("street_adress");

            entity.HasOne(d => d.IdVacancyNavigation).WithMany(p => p.Adresses)
                .HasForeignKey(d => d.IdVacancy)
                .HasConstraintName("adresses_vacancies_id_vacancy_fk");
        });

        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(e => e.IdChat).HasName("chat_pk");

            entity.ToTable("chat", "profnastavnik");

            entity.Property(e => e.IdChat)
                .ValueGeneratedNever()
                .HasColumnName("id_chat");
            entity.Property(e => e.EmailAdmin)
                .HasMaxLength(100)
                .HasColumnName("email_admin");
            entity.Property(e => e.IdDirector).HasColumnName("id_director");
            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.IdVacancy).HasColumnName("id_vacancy");
            entity.Property(e => e.SendAtChat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("send_at_chat");
            entity.Property(e => e.SenderChat)
                .HasMaxLength(30)
                .HasColumnName("sender_chat");
            entity.Property(e => e.StatusChat)
                .HasMaxLength(20)
                .HasColumnName("status_chat");
            entity.Property(e => e.TextChat).HasColumnName("text_chat");

            entity.HasOne(d => d.EmailAdminNavigation).WithMany(p => p.Chats)
                .HasForeignKey(d => d.EmailAdmin)
                .HasConstraintName("chat_users_email_user_fk");

            entity.HasOne(d => d.IdDirectorNavigation).WithMany(p => p.Chats)
                .HasForeignKey(d => d.IdDirector)
                .HasConstraintName("chat_directors_id_director_fk");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Chats)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("chat_students_id_student_fk");

            entity.HasOne(d => d.IdVacancyNavigation).WithMany(p => p.Chats)
                .HasForeignKey(d => d.IdVacancy)
                .HasConstraintName("chat_vacancies_id_vacancy_fk");
        });

        modelBuilder.Entity<CompanyPay>(entity =>
        {
            entity.HasKey(e => e.IdCompanyPay).HasName("company_pays_pk");

            entity.ToTable("company_pays", "profnastavnik");

            entity.Property(e => e.IdCompanyPay)
                .ValueGeneratedNever()
                .HasColumnName("id_company_pay");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.IdCompany).HasColumnName("id_company");
            entity.Property(e => e.IdPay).HasColumnName("id_pay");

            entity.HasOne(d => d.IdCompanyNavigation).WithMany(p => p.CompanyPays)
                .HasForeignKey(d => d.IdCompany)
                .HasConstraintName("company_pays_directors_id_director_fk");

            entity.HasOne(d => d.IdPayNavigation).WithMany(p => p.CompanyPays)
                .HasForeignKey(d => d.IdPay)
                .HasConstraintName("company_pays_prices_for_company_id_fk");
        });

        modelBuilder.Entity<CompanyPrice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("company_prices_pk");

            entity.ToTable("company_prices", "profnastavnik");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.IdCompany).HasColumnName("id_company");
            entity.Property(e => e.IdPrice).HasColumnName("id_price");

            entity.HasOne(d => d.IdCompanyNavigation).WithMany(p => p.CompanyPrices)
                .HasForeignKey(d => d.IdCompany)
                .HasConstraintName("company_prices_directors_id_director_fk");

            entity.HasOne(d => d.IdPriceNavigation).WithMany(p => p.CompanyPrices)
                .HasForeignKey(d => d.IdPrice)
                .HasConstraintName("company_prices_prices_for_company_id_fk");
        });

        modelBuilder.Entity<CompanySfere>(entity =>
        {
            entity.HasKey(e => e.IdCompanySfere).HasName("company_sferes_pk");

            entity.ToTable("company_sferes", "profnastavnik");

            entity.Property(e => e.IdCompanySfere)
                .ValueGeneratedNever()
                .HasColumnName("id_company_sfere");
            entity.Property(e => e.IdCompany).HasColumnName("id_company");
            entity.Property(e => e.IdSfere).HasColumnName("id_sfere");

            entity.HasOne(d => d.IdCompanyNavigation).WithMany(p => p.CompanySferes)
                .HasForeignKey(d => d.IdCompany)
                .HasConstraintName("company_sferes_directors_id_director_fk");

            entity.HasOne(d => d.IdSfereNavigation).WithMany(p => p.CompanySferes)
                .HasForeignKey(d => d.IdSfere)
                .HasConstraintName("company_sferes_sferes_id_sfere_fk");
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.HasKey(e => e.IdCurrency).HasName("currencies_pk");

            entity.ToTable("currencies", "profnastavnik");

            entity.Property(e => e.IdCurrency)
                .ValueGeneratedNever()
                .HasColumnName("id_currency");
            entity.Property(e => e.NameCurrency)
                .HasMaxLength(40)
                .HasColumnName("name_currency");
        });

        modelBuilder.Entity<Deleting>(entity =>
        {
            entity.HasKey(e => e.IdDelete).HasName("deleting_pk");

            entity.ToTable("deleting", "profnastavnik");

            entity.Property(e => e.IdDelete)
                .ValueGeneratedNever()
                .HasColumnName("id_delete");
            entity.Property(e => e.DateDelete).HasColumnName("date_delete");
            entity.Property(e => e.EmailUser)
                .HasMaxLength(100)
                .HasColumnName("email_user");

            entity.HasOne(d => d.EmailUserNavigation).WithMany(p => p.Deletings)
                .HasForeignKey(d => d.EmailUser)
                .HasConstraintName("deleting_users_email_user_fk");
        });

        modelBuilder.Entity<Director>(entity =>
        {
            entity.HasKey(e => e.IdDirector).HasName("directors_pk");

            entity.ToTable("directors", "profnastavnik");

            entity.Property(e => e.IdDirector)
                .ValueGeneratedNever()
                .HasColumnName("id_director");
            entity.Property(e => e.AuthenticationDirector).HasColumnName("authentication_director");
            entity.Property(e => e.CityDirector)
                .HasMaxLength(60)
                .HasColumnName("city_director");
            entity.Property(e => e.DateCreatedDirector).HasColumnName("date_created_director");
            entity.Property(e => e.DescriptionDirector).HasColumnName("description_director");
            entity.Property(e => e.EmailDirector)
                .HasMaxLength(100)
                .HasColumnName("email_director");
            entity.Property(e => e.NameCompanyDirector)
                .HasMaxLength(100)
                .HasColumnName("name_company_director");
            entity.Property(e => e.NameDirector)
                .HasMaxLength(50)
                .HasColumnName("name_director");
            entity.Property(e => e.PatronymicDirector)
                .HasMaxLength(50)
                .HasColumnName("patronymic_director");
            entity.Property(e => e.PhoneDirector)
                .HasMaxLength(25)
                .HasColumnName("phone_director");
            entity.Property(e => e.StatusDirector)
                .HasMaxLength(50)
                .HasColumnName("status_director");
            entity.Property(e => e.SurnameDirector)
                .HasMaxLength(50)
                .HasColumnName("surname_director");
            entity.Property(e => e.TimeCreatedDirector).HasColumnName("time_created_director");
            entity.Property(e => e.WebsiteDirector).HasColumnName("website_director");

            entity.HasOne(d => d.EmailDirectorNavigation).WithMany(p => p.Directors)
                .HasForeignKey(d => d.EmailDirector)
                .HasConstraintName("directors_users_email_user_fk");
        });

        modelBuilder.Entity<Favourity>(entity =>
        {
            entity.HasKey(e => e.IdFavourity).HasName("favourities_pk");

            entity.ToTable("favourities", "profnastavnik");

            entity.Property(e => e.IdFavourity)
                .ValueGeneratedNever()
                .HasColumnName("id_favourity");
            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.IdVacancy).HasColumnName("id_vacancy");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Favourities)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("favourities_students_id_student_fk");

            entity.HasOne(d => d.IdVacancyNavigation).WithMany(p => p.Favourities)
                .HasForeignKey(d => d.IdVacancy)
                .HasConstraintName("favourities_vacancies_id_vacancy_fk");
        });

        modelBuilder.Entity<FeedbacksCompany>(entity =>
        {
            entity.HasKey(e => e.IdFeedbackCompany).HasName("feedbacks_company_pk");

            entity.ToTable("feedbacks_company", "profnastavnik");

            entity.Property(e => e.IdFeedbackCompany)
                .ValueGeneratedNever()
                .HasColumnName("id_feedback_company");
            entity.Property(e => e.DateFeedback).HasColumnName("date_feedback");
            entity.Property(e => e.DescriptionCompany).HasColumnName("description_company");
            entity.Property(e => e.IdCompany).HasColumnName("id_company");
            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.IdVacancy).HasColumnName("id_vacancy");
            entity.Property(e => e.RatingStudent).HasColumnName("rating_student");
            entity.Property(e => e.StatusFeedbackCompany)
                .HasMaxLength(20)
                .HasColumnName("status_feedback_company");

            entity.HasOne(d => d.IdCompanyNavigation).WithMany(p => p.FeedbacksCompanies)
                .HasForeignKey(d => d.IdCompany)
                .HasConstraintName("feedbacks_company_directors_id_director_fk");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.FeedbacksCompanies)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("feedbacks_company_students_id_student_fk");

            entity.HasOne(d => d.IdVacancyNavigation).WithMany(p => p.FeedbacksCompanies)
                .HasForeignKey(d => d.IdVacancy)
                .HasConstraintName("feedbacks_company_vacancies_id_vacancy_fk");
        });

        modelBuilder.Entity<FeedbacksUser>(entity =>
        {
            entity.HasKey(e => e.IdFeedbackUser).HasName("feedbacks_user_pk");

            entity.ToTable("feedbacks_user", "profnastavnik");

            entity.Property(e => e.IdFeedbackUser)
                .ValueGeneratedNever()
                .HasColumnName("id_feedback_user");
            entity.Property(e => e.DateFeedback).HasColumnName("date_feedback");
            entity.Property(e => e.DescriptionFeedbackUser).HasColumnName("description_feedback_user");
            entity.Property(e => e.IdCompany).HasColumnName("id_company");
            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.IdVacancy).HasColumnName("id_vacancy");
            entity.Property(e => e.RatingFeedbackUser).HasColumnName("rating_feedback_user");
            entity.Property(e => e.StatusFeedbackUser)
                .HasMaxLength(20)
                .HasColumnName("status_feedback_user");

            entity.HasOne(d => d.IdCompanyNavigation).WithMany(p => p.FeedbacksUsers)
                .HasForeignKey(d => d.IdCompany)
                .HasConstraintName("feedbacks_user_directors_id_director_fk");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.FeedbacksUsers)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("feedbacks_user_students_id_student_fk");

            entity.HasOne(d => d.IdVacancyNavigation).WithMany(p => p.FeedbacksUsers)
                .HasForeignKey(d => d.IdVacancy)
                .HasConstraintName("feedbacks_user_vacancies_id_vacancy_fk");
        });

        modelBuilder.Entity<Intership>(entity =>
        {
            entity.HasKey(e => e.IdIntership).HasName("interships_pk");

            entity.ToTable("interships", "profnastavnik");

            entity.Property(e => e.IdIntership)
                .ValueGeneratedNever()
                .HasColumnName("id_intership");
            entity.Property(e => e.DateEndIntership).HasColumnName("date_end_intership");
            entity.Property(e => e.DateStartIntership).HasColumnName("date_start_intership");
            entity.Property(e => e.IdCompany).HasColumnName("id_company");
            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.IdVacancy).HasColumnName("id_vacancy");
            entity.Property(e => e.ResponseId).HasColumnName("response_id");
            entity.Property(e => e.StatusIntership)
                .HasMaxLength(20)
                .HasColumnName("status_intership");

            entity.HasOne(d => d.IdCompanyNavigation).WithMany(p => p.Interships)
                .HasForeignKey(d => d.IdCompany)
                .HasConstraintName("interships_directors_id_director_fk");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Interships)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("interships_students_id_student_fk");

            entity.HasOne(d => d.IdVacancyNavigation).WithMany(p => p.Interships)
                .HasForeignKey(d => d.IdVacancy)
                .HasConstraintName("interships_vacancies_id_vacancy_fk");

            entity.HasOne(d => d.Response).WithMany(p => p.Interships)
                .HasForeignKey(d => d.ResponseId)
                .HasConstraintName("interships_responses_id_response_fk");
        });

        modelBuilder.Entity<MoneyType>(entity =>
        {
            entity.HasKey(e => e.IdMoneyType).HasName("money_types_pk");

            entity.ToTable("money_types", "profnastavnik");

            entity.Property(e => e.IdMoneyType)
                .ValueGeneratedNever()
                .HasColumnName("id_money_type");
            entity.Property(e => e.NameMoneyType)
                .HasMaxLength(60)
                .HasColumnName("name_money_type");
        });

        modelBuilder.Entity<PricesForCompany>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prices_for_company_pk");

            entity.ToTable("prices_for_company", "profnastavnik");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Cost).HasColumnName("cost");
            entity.Property(e => e.CountResponce).HasColumnName("count_responce");
            entity.Property(e => e.CountVacancy).HasColumnName("count_vacancy");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.NamePrice)
                .HasMaxLength(60)
                .HasColumnName("name_price");
        });

        modelBuilder.Entity<PricesForUser>(entity =>
        {
            entity.HasKey(e => e.IdPrice).HasName("prices_for_user_pk");

            entity.ToTable("prices_for_user", "profnastavnik");

            entity.Property(e => e.IdPrice)
                .ValueGeneratedNever()
                .HasColumnName("id_price");
            entity.Property(e => e.CostPrice).HasColumnName("cost_price");
            entity.Property(e => e.CountResponces).HasColumnName("count_responces");
            entity.Property(e => e.DescriptionPrice).HasColumnName("description_price");
            entity.Property(e => e.NamePrice)
                .HasMaxLength(60)
                .HasColumnName("name_price");
        });

        modelBuilder.Entity<Profession>(entity =>
        {
            entity.HasKey(e => e.IdProfession).HasName("professions_pk");

            entity.ToTable("professions", "profnastavnik");

            entity.Property(e => e.IdProfession)
                .ValueGeneratedNever()
                .HasColumnName("id_profession");
            entity.Property(e => e.NameProfession)
                .HasMaxLength(100)
                .HasColumnName("name_profession");
        });

        modelBuilder.Entity<RequestsCompany>(entity =>
        {
            entity.HasKey(e => e.IdRequest).HasName("requests_company_pk");

            entity.ToTable("requests_company", "profnastavnik");

            entity.Property(e => e.IdRequest)
                .ValueGeneratedNever()
                .HasColumnName("id_request");
            entity.Property(e => e.DateRequest).HasColumnName("date_request");
            entity.Property(e => e.DescriptionRequset).HasColumnName("description_requset");
            entity.Property(e => e.IdCompany).HasColumnName("id_company");
            entity.Property(e => e.IdFeedback).HasColumnName("id_feedback");
            entity.Property(e => e.StatusFeedback)
                .HasMaxLength(30)
                .HasColumnName("status_feedback");

            entity.HasOne(d => d.IdCompanyNavigation).WithMany(p => p.RequestsCompanies)
                .HasForeignKey(d => d.IdCompany)
                .HasConstraintName("requests_company_directors_id_director_fk");

            entity.HasOne(d => d.IdFeedbackNavigation).WithMany(p => p.RequestsCompanies)
                .HasForeignKey(d => d.IdFeedback)
                .HasConstraintName("requests_company_feedbacks_user_id_feedback_user_fk");
        });

        modelBuilder.Entity<RequestsUser>(entity =>
        {
            entity.HasKey(e => e.IdRequest).HasName("requests_user_pk");

            entity.ToTable("requests_user", "profnastavnik");

            entity.Property(e => e.IdRequest)
                .ValueGeneratedNever()
                .HasColumnName("id_request");
            entity.Property(e => e.DateRequest).HasColumnName("date_request");
            entity.Property(e => e.DescriptionRequest).HasColumnName("description_request");
            entity.Property(e => e.IdFeedback).HasColumnName("id_feedback");
            entity.Property(e => e.IdStudent).HasColumnName("id_student");
            entity.Property(e => e.StatusRequest)
                .HasMaxLength(30)
                .HasColumnName("status_request");

            entity.HasOne(d => d.IdFeedbackNavigation).WithMany(p => p.RequestsUsers)
                .HasForeignKey(d => d.IdFeedback)
                .HasConstraintName("requests_user_feedbacks_company_id_feedback_company_fk");

            entity.HasOne(d => d.IdStudentNavigation).WithMany(p => p.RequestsUsers)
                .HasForeignKey(d => d.IdStudent)
                .HasConstraintName("requests_user_students_id_student_fk");
        });

        modelBuilder.Entity<Response>(entity =>
        {
            entity.HasKey(e => e.IdResponse).HasName("responses_pk");

            entity.ToTable("responses", "profnastavnik");

            entity.Property(e => e.IdResponse)
                .ValueGeneratedNever()
                .HasColumnName("id_response");
            entity.Property(e => e.DateResponse).HasColumnName("date_response");
            entity.Property(e => e.DescriptionResponse).HasColumnName("description_response");
            entity.Property(e => e.IdCompany).HasColumnName("id_company");
            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.IdVacancy).HasColumnName("id_vacancy");
            entity.Property(e => e.PortfolioResponse)
                .HasMaxLength(100)
                .HasColumnName("portfolio_response");
            entity.Property(e => e.StatusResponse)
                .HasMaxLength(20)
                .HasColumnName("status_response");

            entity.HasOne(d => d.IdCompanyNavigation).WithMany(p => p.Responses)
                .HasForeignKey(d => d.IdCompany)
                .HasConstraintName("responses_directors_id_director_fk");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Responses)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("responses_students_id_student_fk");

            entity.HasOne(d => d.IdVacancyNavigation).WithMany(p => p.Responses)
                .HasForeignKey(d => d.IdVacancy)
                .HasConstraintName("responses_vacancies_id_vacancy_fk");
        });

        modelBuilder.Entity<Sfere>(entity =>
        {
            entity.HasKey(e => e.IdSfere).HasName("sferes_pk");

            entity.ToTable("sferes", "profnastavnik");

            entity.Property(e => e.IdSfere)
                .ValueGeneratedNever()
                .HasColumnName("id_sfere");
            entity.Property(e => e.NameSfere)
                .HasMaxLength(100)
                .HasColumnName("name_sfere");
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.IdSkill).HasName("skills_pk");

            entity.ToTable("skills", "profnastavnik");

            entity.Property(e => e.IdSkill)
                .ValueGeneratedNever()
                .HasColumnName("id_skill");
            entity.Property(e => e.NameSkill)
                .HasMaxLength(100)
                .HasColumnName("name_skill");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.IdStudent).HasName("students_pk");

            entity.ToTable("students", "profnastavnik");

            entity.Property(e => e.IdStudent)
                .ValueGeneratedNever()
                .HasColumnName("id_student");
            entity.Property(e => e.BirthdayStudent).HasColumnName("birthday_student");
            entity.Property(e => e.CourseStudent).HasColumnName("course_student");
            entity.Property(e => e.DateCreatedStudent).HasColumnName("date_created_student");
            entity.Property(e => e.EmailStudent)
                .HasMaxLength(100)
                .HasColumnName("email_student");
            entity.Property(e => e.FacultatyStudent)
                .HasMaxLength(100)
                .HasColumnName("facultaty_student");
            entity.Property(e => e.NameStudent)
                .HasMaxLength(50)
                .HasColumnName("name_student");
            entity.Property(e => e.PatronymicStudent)
                .HasMaxLength(50)
                .HasColumnName("patronymic_student");
            entity.Property(e => e.PhotoStudent)
                .HasMaxLength(100)
                .HasColumnName("photo_student");
            entity.Property(e => e.StatusStudent)
                .HasMaxLength(50)
                .HasColumnName("status_student");
            entity.Property(e => e.SurnameStudent)
                .HasMaxLength(50)
                .HasColumnName("surname_student");
            entity.Property(e => e.TimeCreatedStudent).HasColumnName("time_created_student");
            entity.Property(e => e.UniversityStudent).HasColumnName("university_student");

            entity.HasOne(d => d.EmailStudentNavigation).WithMany(p => p.Students)
                .HasForeignKey(d => d.EmailStudent)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("students_users_email_user_fk");

            entity.HasOne(d => d.UniversityStudentNavigation).WithMany(p => p.Students)
                .HasForeignKey(d => d.UniversityStudent)
                .HasConstraintName("students_universities_id_university_fk");
        });

        modelBuilder.Entity<StudentFile>(entity =>
        {
            entity.HasKey(e => e.IdStudentFile).HasName("student_files_pk");

            entity.ToTable("student_files", "profnastavnik");

            entity.Property(e => e.IdStudentFile)
                .ValueGeneratedNever()
                .HasColumnName("id_student_file");
            entity.Property(e => e.FileUrl).HasColumnName("file_url");
            entity.Property(e => e.IdUser).HasColumnName("id_user");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.StudentFiles)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("student_files_students_id_student_fk");
        });

        modelBuilder.Entity<University>(entity =>
        {
            entity.HasKey(e => e.IdUniversity).HasName("universities_pk");

            entity.ToTable("universities", "profnastavnik");

            entity.Property(e => e.IdUniversity)
                .ValueGeneratedNever()
                .HasColumnName("id_university");
            entity.Property(e => e.NameUniversity)
                .HasMaxLength(150)
                .HasColumnName("name_university");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.EmailUser).HasName("users_pk");

            entity.ToTable("users", "profnastavnik");

            entity.Property(e => e.EmailUser)
                .HasMaxLength(100)
                .HasColumnName("email_user");
            entity.Property(e => e.PasswordUser).HasColumnName("password_user");
            entity.Property(e => e.RoleUser)
                .HasMaxLength(15)
                .HasColumnName("role_user");
        });

        modelBuilder.Entity<UserConsent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_consents_pkey");

            entity.ToTable("user_consents", "profnastavnik");

            entity.HasIndex(e => e.Email, "ix_user_consents_email");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AcceptedAt).HasColumnName("accepted_at");
            entity.Property(e => e.ConsentVersion).HasColumnName("consent_version");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Ip).HasColumnName("ip");
            entity.Property(e => e.IsAdult).HasColumnName("is_adult");
        });

        modelBuilder.Entity<UserPay>(entity =>
        {
            entity.HasKey(e => e.IdUserPay).HasName("user_pays_pk");

            entity.ToTable("user_pays", "profnastavnik");

            entity.Property(e => e.IdUserPay)
                .ValueGeneratedNever()
                .HasColumnName("id_user_pay");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.IdPrice).HasColumnName("id_price");
            entity.Property(e => e.IdUser).HasColumnName("id_user");

            entity.HasOne(d => d.IdPriceNavigation).WithMany(p => p.UserPays)
                .HasForeignKey(d => d.IdPrice)
                .HasConstraintName("user_pays_prices_for_user_id_price_fk");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.UserPays)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("user_pays_students_id_student_fk");
        });

        modelBuilder.Entity<UserPrice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_prices_pk");

            entity.ToTable("user_prices", "profnastavnik");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.IdPrices).HasColumnName("id_prices");
            entity.Property(e => e.IdUser).HasColumnName("id_user");

            entity.HasOne(d => d.IdPricesNavigation).WithMany(p => p.UserPrices)
                .HasForeignKey(d => d.IdPrices)
                .HasConstraintName("user_prices_prices_for_user_id_price_fk");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.UserPrices)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("user_prices_students_id_student_fk");
        });

        modelBuilder.Entity<UserProfession>(entity =>
        {
            entity.HasKey(e => e.IdUserProfession).HasName("user_professions_pk");

            entity.ToTable("user_professions", "profnastavnik");

            entity.Property(e => e.IdUserProfession)
                .ValueGeneratedNever()
                .HasColumnName("id_user_profession");
            entity.Property(e => e.IdProfession).HasColumnName("id_profession");
            entity.Property(e => e.IdUser).HasColumnName("id_user");

            entity.HasOne(d => d.IdProfessionNavigation).WithMany(p => p.UserProfessions)
                .HasForeignKey(d => d.IdProfession)
                .HasConstraintName("user_professions_professions_id_profession_fk");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.UserProfessions)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("user_professions_students_id_student_fk");
        });

        modelBuilder.Entity<UserSkill>(entity =>
        {
            entity.HasKey(e => e.IdUserSkill).HasName("user_skills_pk");

            entity.ToTable("user_skills", "profnastavnik");

            entity.Property(e => e.IdUserSkill)
                .ValueGeneratedNever()
                .HasColumnName("id_user_skill");
            entity.Property(e => e.IdSkill).HasColumnName("id_skill");
            entity.Property(e => e.IdUser).HasColumnName("id_user");

            entity.HasOne(d => d.IdSkillNavigation).WithMany(p => p.UserSkills)
                .HasForeignKey(d => d.IdSkill)
                .HasConstraintName("user_skills_skills_id_skill_fk");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.UserSkills)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("user_skills_students_id_student_fk");
        });

        modelBuilder.Entity<VacanciesFormat>(entity =>
        {
            entity.HasKey(e => e.IdVacancyFormat).HasName("vacancies_formats_pk");

            entity.ToTable("vacancies_formats", "profnastavnik");

            entity.Property(e => e.IdVacancyFormat)
                .ValueGeneratedNever()
                .HasColumnName("id_vacancy_format");
            entity.Property(e => e.IdFormat).HasColumnName("id_format");
            entity.Property(e => e.IdVacancy).HasColumnName("id_vacancy");

            entity.HasOne(d => d.IdFormatNavigation).WithMany(p => p.VacanciesFormats)
                .HasForeignKey(d => d.IdFormat)
                .HasConstraintName("vacancies_formats_vacancy_format_id_vacancy_format_fk");

            entity.HasOne(d => d.IdVacancyNavigation).WithMany(p => p.VacanciesFormats)
                .HasForeignKey(d => d.IdVacancy)
                .HasConstraintName("vacancies_formats_vacancies_id_vacancy_fk");
        });

        modelBuilder.Entity<Vacancy>(entity =>
        {
            entity.HasKey(e => e.IdVacancy).HasName("vacancies_pk");

            entity.ToTable("vacancies", "profnastavnik");

            entity.Property(e => e.IdVacancy)
                .ValueGeneratedNever()
                .HasColumnName("id_vacancy");
            entity.Property(e => e.AnybodyVacancy).HasColumnName("anybody_vacancy");
            entity.Property(e => e.CountUserVacancy).HasColumnName("count_user_vacancy");
            entity.Property(e => e.CurrencyVacancy).HasColumnName("currency_vacancy");
            entity.Property(e => e.DateCreated).HasColumnName("date_created");
            entity.Property(e => e.DescroptionVacancy).HasColumnName("descroption_vacancy");
            entity.Property(e => e.IdCompany).HasColumnName("id_company");
            entity.Property(e => e.MoneyTypeVacancy).HasColumnName("money_type_vacancy");
            entity.Property(e => e.NameVacancy)
                .HasMaxLength(100)
                .HasColumnName("name_vacancy");
            entity.Property(e => e.StatusVacancy)
                .HasMaxLength(20)
                .HasColumnName("status_vacancy");
            entity.Property(e => e.TimeVacancy)
                .HasMaxLength(20)
                .HasColumnName("time_vacancy");
            entity.Property(e => e.ZenEndVacancy).HasColumnName("zen_end_vacancy");
            entity.Property(e => e.ZenStartVacancy).HasColumnName("zen_start_vacancy");

            entity.HasOne(d => d.CurrencyVacancyNavigation).WithMany(p => p.Vacancies)
                .HasForeignKey(d => d.CurrencyVacancy)
                .HasConstraintName("vacancies_currencies_id_currency_fk");

            entity.HasOne(d => d.IdCompanyNavigation).WithMany(p => p.Vacancies)
                .HasForeignKey(d => d.IdCompany)
                .HasConstraintName("vacancies_directors_id_director_fk");

            entity.HasOne(d => d.MoneyTypeVacancyNavigation).WithMany(p => p.Vacancies)
                .HasForeignKey(d => d.MoneyTypeVacancy)
                .HasConstraintName("vacancies_money_types_id_money_type_fk");
        });

        modelBuilder.Entity<VacancyFormat>(entity =>
        {
            entity.HasKey(e => e.IdVacancyFormat).HasName("vacancy_format_pk");

            entity.ToTable("vacancy_format", "profnastavnik");

            entity.Property(e => e.IdVacancyFormat)
                .ValueGeneratedNever()
                .HasColumnName("id_vacancy_format");
            entity.Property(e => e.NameVacancyFormat)
                .HasMaxLength(60)
                .HasColumnName("name_vacancy_format");
        });

        modelBuilder.Entity<VacancySfere>(entity =>
        {
            entity.HasKey(e => e.IdVacationSfere).HasName("vacancy_sferes_pk");

            entity.ToTable("vacancy_sferes", "profnastavnik");

            entity.Property(e => e.IdVacationSfere)
                .ValueGeneratedNever()
                .HasColumnName("id_vacation_sfere");
            entity.Property(e => e.IdSfere).HasColumnName("id_sfere");
            entity.Property(e => e.IdVacancy).HasColumnName("id_vacancy");

            entity.HasOne(d => d.IdSfereNavigation).WithMany(p => p.VacancySferes)
                .HasForeignKey(d => d.IdSfere)
                .HasConstraintName("vacancy_sferes_sferes_id_sfere_fk");

            entity.HasOne(d => d.IdVacancyNavigation).WithMany(p => p.VacancySferes)
                .HasForeignKey(d => d.IdVacancy)
                .HasConstraintName("vacancy_sferes_vacancies_id_vacancy_fk");
        });

        modelBuilder.Entity<VacancySkill>(entity =>
        {
            entity.HasKey(e => e.IdVacationSkill).HasName("vacancy_skills_pk");

            entity.ToTable("vacancy_skills", "profnastavnik");

            entity.Property(e => e.IdVacationSkill)
                .ValueGeneratedNever()
                .HasColumnName("id_vacation_skill");
            entity.Property(e => e.IdSkill).HasColumnName("id_skill");
            entity.Property(e => e.IdVacancy).HasColumnName("id_vacancy");

            entity.HasOne(d => d.IdSkillNavigation).WithMany(p => p.VacancySkills)
                .HasForeignKey(d => d.IdSkill)
                .HasConstraintName("vacancy_skills_skills_id_skill_fk");

            entity.HasOne(d => d.IdVacancyNavigation).WithMany(p => p.VacancySkills)
                .HasForeignKey(d => d.IdVacancy)
                .HasConstraintName("vacancy_skills_vacancies_id_vacancy_fk");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
