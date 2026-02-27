-- ============================================================
-- SCRIPT BỔ SUNG CƠ SỞ DỮ LIỆU - Quản Lý Tiến Độ Sinh Viên
-- Chạy script này trên database hiện tại để bổ sung các bảng
-- và dữ liệu mẫu cần thiết cho Phases 2-4
-- ============================================================

-- ============================================================
-- PHẦN 1: TẠO BẢNG MỚI
-- ============================================================

-- 1. Bảng liên kết Ngành - Môn học (cho Curriculum / Chương trình đào tạo)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'major_subjects')
BEGIN
    CREATE TABLE major_subjects (
        id INT IDENTITY(1,1) PRIMARY KEY,
        major_id INT NOT NULL,
        subject_id INT NOT NULL,
        semester_order INT NULL,  -- Học kỳ khuyến nghị (1-8)
        is_required BIT DEFAULT 1, -- Bắt buộc hay tự chọn
        CONSTRAINT FK_ms_majors FOREIGN KEY (major_id) REFERENCES majors(id),
        CONSTRAINT FK_ms_subjects FOREIGN KEY (subject_id) REFERENCES subjects(id),
        CONSTRAINT UQ_major_subject UNIQUE (major_id, subject_id)
    );
    PRINT N'✅ Đã tạo bảng major_subjects';
END
ELSE
    PRINT N'⏩ Bảng major_subjects đã tồn tại';

-- 2. Bảng Bài tập (cho Teacher/Assignments)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'assignments')
BEGIN
    CREATE TABLE assignments (
        id INT IDENTITY(1,1) PRIMARY KEY,
        subject_id INT NOT NULL,
        class_id INT NOT NULL,
        lecturer_id INT NOT NULL,
        title NVARCHAR(200) NOT NULL,
        description NVARCHAR(MAX) NULL,
        due_date DATETIME NULL,
        max_score FLOAT DEFAULT 10,
        created_at DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_assignments_subjects FOREIGN KEY (subject_id) REFERENCES subjects(id),
        CONSTRAINT FK_assignments_classes FOREIGN KEY (class_id) REFERENCES classes(id),
        CONSTRAINT FK_assignments_lecturers FOREIGN KEY (lecturer_id) REFERENCES lecturers(id)
    );
    PRINT N'✅ Đã tạo bảng assignments';
END
ELSE
    PRINT N'⏩ Bảng assignments đã tồn tại';

-- 3. Bảng Nộp bài / Chấm điểm bài tập
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'assignment_submissions')
BEGIN
    CREATE TABLE assignment_submissions (
        id INT IDENTITY(1,1) PRIMARY KEY,
        assignment_id INT NOT NULL,
        student_id INT NOT NULL,
        score FLOAT NULL,
        comment NVARCHAR(MAX) NULL,
        submitted_at DATETIME DEFAULT GETDATE(),
        graded_at DATETIME NULL,
        CONSTRAINT FK_as_assignments FOREIGN KEY (assignment_id) REFERENCES assignments(id),
        CONSTRAINT FK_as_students FOREIGN KEY (student_id) REFERENCES students(id),
        CONSTRAINT UQ_assignment_student UNIQUE (assignment_id, student_id)
    );
    PRINT N'✅ Đã tạo bảng assignment_submissions';
END
ELSE
    PRINT N'⏩ Bảng assignment_submissions đã tồn tại';

-- ============================================================
-- PHẦN 2: DỮ LIỆU MẪU
-- Chỉ insert nếu chưa có dữ liệu
-- ============================================================

-- 2.1 Ngành học mẫu
IF NOT EXISTS (SELECT 1 FROM majors)
BEGIN
    INSERT INTO majors (name) VALUES
        (N'Công nghệ thông tin'),
        (N'Khoa học máy tính'),
        (N'Kỹ thuật phần mềm');
    PRINT N'✅ Đã thêm 3 ngành học mẫu';
END

-- 2.2 Lớp học mẫu
IF NOT EXISTS (SELECT 1 FROM classes)
BEGIN
    DECLARE @majorCNTT INT = (SELECT TOP 1 id FROM majors WHERE name LIKE N'%Công nghệ thông tin%');
    DECLARE @majorKHMT INT = (SELECT TOP 1 id FROM majors WHERE name LIKE N'%Khoa học máy tính%');
    DECLARE @majorKTPM INT = (SELECT TOP 1 id FROM majors WHERE name LIKE N'%Kỹ thuật phần mềm%');

    INSERT INTO classes (name, major_id) VALUES
        (N'CNTT2022A', ISNULL(@majorCNTT, 1)),
        (N'CNTT2022B', ISNULL(@majorCNTT, 1)),
        (N'KHMT2022A', ISNULL(@majorKHMT, 2)),
        (N'KTPM2022A', ISNULL(@majorKTPM, 3));
    PRINT N'✅ Đã thêm 4 lớp học mẫu';
END

-- 2.3 Học kỳ mẫu
IF NOT EXISTS (SELECT 1 FROM semesters)
BEGIN
    INSERT INTO semesters (name, start_date, end_date) VALUES
        (N'HK1 2023-2024', '2023-09-01', '2024-01-15'),
        (N'HK2 2023-2024', '2024-02-01', '2024-06-15'),
        (N'HK1 2024-2025', '2024-09-01', '2025-01-15'),
        (N'HK2 2024-2025', '2025-02-01', '2025-06-15'),
        (N'HK1 2025-2026', '2025-09-01', '2026-01-15'),
        (N'HK2 2025-2026', '2026-02-01', '2026-06-15');
    PRINT N'✅ Đã thêm 6 học kỳ mẫu';
END

-- 2.4 Môn học mẫu
IF NOT EXISTS (SELECT 1 FROM subjects)
BEGIN
    INSERT INTO subjects (code, name, credit) VALUES
        ('CS101', N'Nhập môn lập trình', 3),
        ('CS102', N'Cấu trúc dữ liệu và giải thuật', 4),
        ('CS201', N'Cơ sở dữ liệu', 3),
        ('CS202', N'Lập trình hướng đối tượng', 3),
        ('CS301', N'Mạng máy tính', 3),
        ('CS302', N'Công nghệ phần mềm', 3),
        ('CS303', N'Trí tuệ nhân tạo', 3),
        ('CS401', N'Phát triển ứng dụng Web', 4),
        ('CS402', N'An toàn thông tin', 3),
        ('CS403', N'Đồ án tốt nghiệp', 6),
        ('MATH101', N'Giải tích 1', 3),
        ('MATH102', N'Đại số tuyến tính', 3),
        ('MATH201', N'Xác suất thống kê', 3),
        ('PHY101', N'Vật lý đại cương', 3),
        ('ENG101', N'Tiếng Anh cơ bản', 2);
    PRINT N'✅ Đã thêm 15 môn học mẫu';
END

-- 2.5 Liên kết Ngành - Môn học (Chương trình đào tạo)
IF NOT EXISTS (SELECT 1 FROM major_subjects)
BEGIN
    DECLARE @mCNTT INT = (SELECT TOP 1 id FROM majors WHERE name LIKE N'%Công nghệ thông tin%');
    DECLARE @mKHMT INT = (SELECT TOP 1 id FROM majors WHERE name LIKE N'%Khoa học máy tính%');
    
    -- Lấy subject IDs
    DECLARE @sCS101 INT = (SELECT id FROM subjects WHERE code = 'CS101');
    DECLARE @sCS102 INT = (SELECT id FROM subjects WHERE code = 'CS102');
    DECLARE @sCS201 INT = (SELECT id FROM subjects WHERE code = 'CS201');
    DECLARE @sCS202 INT = (SELECT id FROM subjects WHERE code = 'CS202');
    DECLARE @sCS301 INT = (SELECT id FROM subjects WHERE code = 'CS301');
    DECLARE @sCS302 INT = (SELECT id FROM subjects WHERE code = 'CS302');
    DECLARE @sCS303 INT = (SELECT id FROM subjects WHERE code = 'CS303');
    DECLARE @sCS401 INT = (SELECT id FROM subjects WHERE code = 'CS401');
    DECLARE @sCS402 INT = (SELECT id FROM subjects WHERE code = 'CS402');
    DECLARE @sCS403 INT = (SELECT id FROM subjects WHERE code = 'CS403');
    DECLARE @sMATH101 INT = (SELECT id FROM subjects WHERE code = 'MATH101');
    DECLARE @sMATH102 INT = (SELECT id FROM subjects WHERE code = 'MATH102');

    IF @mCNTT IS NOT NULL AND @sCS101 IS NOT NULL
    BEGIN
        INSERT INTO major_subjects (major_id, subject_id, semester_order, is_required) VALUES
            (@mCNTT, @sCS101, 1, 1),
            (@mCNTT, @sMATH101, 1, 1),
            (@mCNTT, @sMATH102, 1, 1),
            (@mCNTT, @sCS102, 2, 1),
            (@mCNTT, @sCS201, 3, 1),
            (@mCNTT, @sCS202, 3, 1),
            (@mCNTT, @sCS301, 4, 1),
            (@mCNTT, @sCS302, 5, 1),
            (@mCNTT, @sCS303, 5, 0),
            (@mCNTT, @sCS401, 6, 1),
            (@mCNTT, @sCS402, 6, 0),
            (@mCNTT, @sCS403, 8, 1);
        PRINT N'✅ Đã thêm chương trình đào tạo CNTT';
    END

    IF @mKHMT IS NOT NULL AND @sCS101 IS NOT NULL
    BEGIN
        INSERT INTO major_subjects (major_id, subject_id, semester_order, is_required) VALUES
            (@mKHMT, @sCS101, 1, 1),
            (@mKHMT, @sMATH101, 1, 1),
            (@mKHMT, @sCS102, 2, 1),
            (@mKHMT, @sCS201, 3, 1),
            (@mKHMT, @sCS303, 4, 1),
            (@mKHMT, @sCS401, 5, 0);
        PRINT N'✅ Đã thêm chương trình đào tạo KHMT';
    END
END

-- 2.6 Tài khoản giảng viên mẫu
IF NOT EXISTS (SELECT 1 FROM users WHERE username = 'gv01')
BEGIN
    DECLARE @teacherRoleId INT = (SELECT TOP 1 id FROM roles WHERE name = 'Teacher');
    IF @teacherRoleId IS NULL
        SET @teacherRoleId = (SELECT TOP 1 id FROM roles WHERE name = 'Lecturer');

    IF @teacherRoleId IS NOT NULL
    BEGIN
        -- Password: teacher123 (SHA256)
        DECLARE @teacherHash NVARCHAR(255) = 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=';

        INSERT INTO users (username, password_hash, role_id, is_active, failed_login, created_at) VALUES
            ('gv01', @teacherHash, @teacherRoleId, 1, 0, GETDATE()),
            ('gv02', @teacherHash, @teacherRoleId, 1, 0, GETDATE());

        DECLARE @gv01UserId INT = (SELECT id FROM users WHERE username = 'gv01');
        DECLARE @gv02UserId INT = (SELECT id FROM users WHERE username = 'gv02');

        INSERT INTO lecturers (user_id, full_name, email, phone) VALUES
            (@gv01UserId, N'Nguyễn Văn Hùng', 'hung.nv@edu.vn', '0901234567'),
            (@gv02UserId, N'Trần Thị Mai', 'mai.tt@edu.vn', '0909876543');
        PRINT N'✅ Đã thêm 2 giảng viên mẫu (gv01/teacher123, gv02/teacher123)';
    END
END

-- 2.7 Tài khoản sinh viên mẫu
IF NOT EXISTS (SELECT 1 FROM users WHERE username = 'sv01')
BEGIN
    DECLARE @studentRoleId INT = (SELECT TOP 1 id FROM roles WHERE name = 'Student');
    DECLARE @classId1 INT = (SELECT TOP 1 id FROM classes);

    IF @studentRoleId IS NOT NULL
    BEGIN
        -- Password: student123 (SHA256)
        DECLARE @studentHash NVARCHAR(255) = 'PMDApzUaScKzy0bBV5GH5sMwCfzzhOAAgcgsHoR4v2c=';

        INSERT INTO users (username, password_hash, role_id, is_active, failed_login, created_at) VALUES
            ('sv01', @studentHash, @studentRoleId, 1, 0, GETDATE()),
            ('sv02', @studentHash, @studentRoleId, 1, 0, GETDATE()),
            ('sv03', @studentHash, @studentRoleId, 1, 0, GETDATE());

        DECLARE @sv01UserId INT = (SELECT id FROM users WHERE username = 'sv01');
        DECLARE @sv02UserId INT = (SELECT id FROM users WHERE username = 'sv02');
        DECLARE @sv03UserId INT = (SELECT id FROM users WHERE username = 'sv03');

        INSERT INTO students (user_id, student_code, full_name, email, phone, address, class_id) VALUES
            (@sv01UserId, 'SV2022001', N'Phạm Minh Tuấn', 'tuan.pm@sv.edu.vn', '0912345678', N'HCM', @classId1),
            (@sv02UserId, 'SV2022002', N'Lê Thị Hương', 'huong.lt@sv.edu.vn', '0923456789', N'Hà Nội', @classId1),
            (@sv03UserId, 'SV2022003', N'Võ Đình Khoa', 'khoa.vd@sv.edu.vn', '0934567890', N'Đà Nẵng', @classId1);
        PRINT N'✅ Đã thêm 3 sinh viên mẫu (sv01/student123, sv02, sv03)';
    END
END

-- 2.8 Phân công giảng viên mẫu
IF NOT EXISTS (SELECT 1 FROM lecturer_assignments)
BEGIN
    DECLARE @lecId1 INT = (SELECT TOP 1 id FROM lecturers);
    DECLARE @lecId2 INT = (SELECT TOP 1 id FROM lecturers WHERE id <> (SELECT TOP 1 id FROM lecturers));
    DECLARE @clsId INT = (SELECT TOP 1 id FROM classes);
    DECLARE @subjId1 INT = (SELECT TOP 1 id FROM subjects);
    DECLARE @subjId2 INT = (SELECT TOP 1 id FROM subjects WHERE id <> (SELECT TOP 1 id FROM subjects));

    IF @lecId1 IS NOT NULL AND @clsId IS NOT NULL AND @subjId1 IS NOT NULL
    BEGIN
        INSERT INTO lecturer_assignments (lecturer_id, subject_id, class_id) VALUES
            (@lecId1, @subjId1, @clsId),
            (@lecId1, @subjId2, @clsId);
        
        IF @lecId2 IS NOT NULL
        BEGIN
            DECLARE @subjId3 INT = (SELECT TOP 1 id FROM subjects WHERE id NOT IN (
                SELECT TOP 2 id FROM subjects ORDER BY id
            ));
            IF @subjId3 IS NOT NULL
                INSERT INTO lecturer_assignments (lecturer_id, subject_id, class_id) VALUES
                    (@lecId2, @subjId3, @clsId);
        END
        PRINT N'✅ Đã thêm phân công giảng viên mẫu';
    END
END

-- 2.9 Đăng ký học (Enrollments) mẫu
IF NOT EXISTS (SELECT 1 FROM enrollments)
BEGIN
    DECLARE @stId1 INT = (SELECT TOP 1 id FROM students);
    DECLARE @stId2 INT = (SELECT TOP 1 id FROM students WHERE id <> (SELECT TOP 1 id FROM students));
    DECLARE @semId1 INT = (SELECT TOP 1 id FROM semesters ORDER BY start_date);
    DECLARE @semId2 INT = (SELECT TOP 1 id FROM semesters WHERE id <> @semId1 ORDER BY start_date);
    DECLARE @sub1 INT = (SELECT id FROM subjects WHERE code = 'CS101');
    DECLARE @sub2 INT = (SELECT id FROM subjects WHERE code = 'CS102');
    DECLARE @sub3 INT = (SELECT id FROM subjects WHERE code = 'MATH101');
    DECLARE @sub4 INT = (SELECT id FROM subjects WHERE code = 'CS201');

    IF @stId1 IS NOT NULL AND @semId1 IS NOT NULL AND @sub1 IS NOT NULL
    BEGIN
        INSERT INTO enrollments (student_id, subject_id, semester_id, status) VALUES
            (@stId1, @sub1, @semId1, 'Completed'),
            (@stId1, @sub3, @semId1, 'Completed'),
            (@stId1, @sub2, @semId2, 'In Progress'),
            (@stId1, @sub4, @semId2, 'In Progress');

        IF @stId2 IS NOT NULL
        BEGIN
            INSERT INTO enrollments (student_id, subject_id, semester_id, status) VALUES
                (@stId2, @sub1, @semId1, 'Completed'),
                (@stId2, @sub3, @semId1, 'In Progress');
        END

        -- Study Progress cho các enrollment đã hoàn thành
        DECLARE @enrId1 INT = (SELECT TOP 1 id FROM enrollments WHERE student_id = @stId1 AND subject_id = @sub1);
        DECLARE @enrId2 INT = (SELECT TOP 1 id FROM enrollments WHERE student_id = @stId1 AND subject_id = @sub3);
        DECLARE @enrId3 INT = (SELECT TOP 1 id FROM enrollments WHERE student_id = @stId1 AND subject_id = @sub2);
        DECLARE @enrId4 INT = (SELECT TOP 1 id FROM enrollments WHERE student_id = @stId1 AND subject_id = @sub4);

        IF @enrId1 IS NOT NULL
        BEGIN
            INSERT INTO study_progress (enrollment_id, score, completion_percent, updated_at) VALUES
                (@enrId1, 8.5, 100, GETDATE()),
                (@enrId2, 7.0, 100, GETDATE()),
                (@enrId3, NULL, 60, GETDATE()),
                (@enrId4, NULL, 40, GETDATE());
        END

        IF @stId2 IS NOT NULL
        BEGIN
            DECLARE @enrId5 INT = (SELECT TOP 1 id FROM enrollments WHERE student_id = @stId2 AND subject_id = @sub1);
            IF @enrId5 IS NOT NULL
                INSERT INTO study_progress (enrollment_id, score, completion_percent, updated_at) VALUES
                    (@enrId5, 9.0, 100, GETDATE());
        END

        PRINT N'✅ Đã thêm enrollments và study_progress mẫu';
    END
END

-- 2.10 Kế hoạch học tập mẫu
IF NOT EXISTS (SELECT 1 FROM study_plans)
BEGIN
    DECLARE @spStId INT = (SELECT TOP 1 id FROM students);
    DECLARE @spSemId INT = (SELECT TOP 1 id FROM semesters ORDER BY start_date DESC);
    DECLARE @spSub1 INT = (SELECT id FROM subjects WHERE code = 'CS301');
    DECLARE @spSub2 INT = (SELECT id FROM subjects WHERE code = 'CS302');

    IF @spStId IS NOT NULL AND @spSemId IS NOT NULL
    BEGIN
        INSERT INTO study_plans (student_id, status, created_at) VALUES
            (@spStId, 'Pending', GETDATE()),
            (@spStId, 'Approved', DATEADD(MONTH, -2, GETDATE()));

        DECLARE @planId1 INT = (SELECT TOP 1 id FROM study_plans WHERE status = 'Pending');
        DECLARE @planId2 INT = (SELECT TOP 1 id FROM study_plans WHERE status = 'Approved');

        IF @spSub1 IS NOT NULL
        BEGIN
            INSERT INTO study_plan_details (study_plan_id, subject_id, semester_id) VALUES
                (@planId1, @spSub1, @spSemId),
                (@planId1, @spSub2, @spSemId);
        END

        -- Review cho plan đã approved
        DECLARE @reviewLecId INT = (SELECT TOP 1 id FROM lecturers);
        IF @reviewLecId IS NOT NULL AND @planId2 IS NOT NULL
        BEGIN
            INSERT INTO study_plan_reviews (study_plan_id, lecturer_id, comment, reviewed_at) VALUES
                (@planId2, @reviewLecId, N'Kế hoạch học tập hợp lý. Đã phê duyệt.', DATEADD(MONTH, -1, GETDATE()));
        END

        PRINT N'✅ Đã thêm kế hoạch học tập mẫu';
    END
END

-- 2.11 Vi phạm mẫu
IF NOT EXISTS (SELECT 1 FROM violations)
BEGIN
    DECLARE @vStId INT = (SELECT TOP 1 id FROM students);
    IF @vStId IS NOT NULL
    BEGIN
        INSERT INTO violations (student_id, description, violation_date) VALUES
            (@vStId, N'Vắng học không phép quá 3 buổi môn CS101', '2024-10-15'),
            (@vStId, N'Nộp bài tập trễ deadline môn CS102', '2025-01-20');
        PRINT N'✅ Đã thêm vi phạm mẫu';
    END
END

-- 2.12 Thông báo mẫu
IF NOT EXISTS (SELECT 1 FROM notifications)
BEGIN
    DECLARE @nUserId INT = (SELECT TOP 1 user_id FROM students);
    IF @nUserId IS NOT NULL
    BEGIN
        INSERT INTO notifications (user_id, content, is_read, created_at) VALUES
            (@nUserId, N'Bạn có kế hoạch học tập mới cần xem xét.', 0, GETDATE()),
            (@nUserId, N'Giảng viên đã phê duyệt kế hoạch HK2 của bạn.', 0, DATEADD(DAY, -3, GETDATE())),
            (@nUserId, N'Nhắc nhở: Hạn nộp bài tập CS102 còn 3 ngày.', 1, DATEADD(DAY, -7, GETDATE()));
        PRINT N'✅ Đã thêm thông báo mẫu';
    END
END

-- ============================================================
PRINT N'';
PRINT N'🎉 HOÀN TẤT! Script đã chạy xong.';
PRINT N'';
PRINT N'📋 Bảng mới: major_subjects, assignments, assignment_submissions';
PRINT N'📋 Dữ liệu mẫu: majors, classes, semesters, subjects, users, students, lecturers, enrollments, study_progress, study_plans, violations, notifications';
PRINT N'';
PRINT N'🔑 Tài khoản test:';
PRINT N'   Admin:    admin / admin123';
PRINT N'   Giảng viên: gv01 / teacher123, gv02 / teacher123';
PRINT N'   Sinh viên:  sv01 / student123, sv02 / student123, sv03 / student123';
