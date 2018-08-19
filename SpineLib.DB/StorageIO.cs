using NLog;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace SpineLib.DB
{
    public class StorageIO
    {
        private static Logger logger = LogManager.GetLogger("DBLog");

        private SQLiteConnection connection;

        public StorageIO(string datasource) {
            connection = new SQLiteConnection(datasource);
            connection.Open();
            logger.Info(string.Format("Connection to DB opened - {0}", connection.FileName));
        }

        public void InitDB()
        {
            string pragma = "PRAGMA foreign_key = '1'";
            string patients = "CREATE TABLE patients (id INTEGER PRIMARY KEY AUTOINCREMENT, name VARCHAR(64) NOT NULL, surname VARCHAR(64) NOT NULL, patronymic VARCHAR(64), age INTEGER NOT NULL)";
            string studies = "CREATE TABLE studies (id INTEGER PRIMARY KEY AUTOINCREMENT, studydate DATE NOT NULL, patient_id INTEGER NOT NULL, FOREIGN KEY(patient_id) REFERENCES patients(id))";
            string images = "CREATE TABLE images (id INTEGER PRIMARY KEY AUTOINCREMENT, hash CHAR(32) NOT NULL, study_id INTEGER NOT NULL, state INTEGER NOT NULL, FOREIGN KEY(study_id) REFERENCES studies(id))";

            if (!TablesExists()) {
                SQLiteCommand command = new SQLiteCommand(pragma, connection);
                command.ExecuteNonQuery();
                command = new SQLiteCommand(patients, connection);
                command.ExecuteNonQuery();
                command = new SQLiteCommand(studies, connection);
                command.ExecuteNonQuery();
                command = new SQLiteCommand(images, connection);
                command.ExecuteNonQuery();
                logger.Info(string.Format("New database created - {0}", connection.FileName));

            }

        }

        private bool TablesExists()
        {
            SQLiteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM sqlite_master WHERE type = 'table' AND name = @name";
            cmd.Parameters.Add("@name", System.Data.DbType.String).Value = "patients";
            return (cmd.ExecuteScalar() != null);
        }

        public Patient GetPatient(int id) {
            SQLiteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM patients WHERE id = @id";
            cmd.Parameters.Add("@id", System.Data.DbType.Int32).Value = id;
            SQLiteDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRows) {
                return null;
            }
            else
            {
                if (reader.Read()) {
                    Patient p = new Patient
                    {
                        ID = id,
                        Name = reader["name"].ToString(),
                        Surname = reader["surname"].ToString()
                    };
                    int res_age = 0;
                    try
                    {
                        res_age = int.Parse(reader["age"].ToString());
                    }
                    catch (IndexOutOfRangeException)
                    {
                        res_age = 0;
                    }
                    catch (FormatException)
                    {
                        res_age = 0;
                    }
                    p.Age = res_age;
                    var patr = reader["patronymic"];
                    if (patr != null)
                    {
                        p.Patronymic = patr.ToString();
                    }
                    else {
                        p.Patronymic = "";
                    }
                    reader.Close();

                    logger.Info(string.Format("Read patient - {0}", p));

                    return p;
                }
            }
            return null;
        }

        public List<Patient> GetPatients()
        {
            List<Patient> patients = new List<Patient>();
            SQLiteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM patients";
            SQLiteDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                return patients;
            }
            else
            {
                while (reader.Read())
                {
                    Patient p = new Patient
                    {
                        ID = int.Parse(reader["id"].ToString()),
                        Name = reader["name"].ToString(),
                        Surname = reader["surname"].ToString()
                    };
                    int res_age = 0;
                    try
                    {
                        res_age = int.Parse(reader["age"].ToString());
                    }
                    catch (IndexOutOfRangeException)
                    {
                        res_age = 0;
                    }
                    catch (FormatException)
                    {
                        res_age = 0;
                    }
                    p.Age = res_age;
                    var patr = reader["patronymic"];
                    if (patr != null)
                    {
                        p.Patronymic = patr.ToString();
                    }
                    else
                    {
                        p.Patronymic = "";
                    }
                    patients.Add(p);
                }
            }

            logger.Info(string.Format("Read list of {0} patients", patients.Count));
            return patients;
        }

        public void InsertPatient(Patient patient) {
            SQLiteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO patients (name, surname, patronymic, age) values (@name, @surname, @patronymic, @age)";
            cmd.Parameters.Add("@name", System.Data.DbType.String).Value = patient.Name;
            cmd.Parameters.Add("@surname", System.Data.DbType.String).Value = patient.Surname;
            cmd.Parameters.Add("@patronymic", System.Data.DbType.String).Value = patient.Patronymic;
            cmd.Parameters.Add("@age", System.Data.DbType.Int16).Value = patient.Age;
            cmd.ExecuteNonQuery();
            logger.Info(string.Format("Inserted patient - {0}", patient));
        }

        public void UpdatePatient(Patient patient)
        {
            SQLiteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "UPDATE patients SET name = @name, surname = @surname, patronymic = @patronymic, age = @age WHERE id = @id";
            cmd.Parameters.Add("@name", System.Data.DbType.String).Value = patient.Name;
            cmd.Parameters.Add("@surname", System.Data.DbType.String).Value = patient.Surname;
            cmd.Parameters.Add("@patronymic", System.Data.DbType.String).Value = patient.Patronymic;
            cmd.Parameters.Add("@age", System.Data.DbType.Int16).Value = patient.Age;
            cmd.Parameters.Add("@id", System.Data.DbType.Int32).Value = patient.ID;
            cmd.ExecuteNonQuery();
            logger.Info(string.Format("Updated patient - {0}", patient));
        }

        public void DeletePatient(Patient patient)
        {
            SQLiteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM patients WHERE id = @id";
            cmd.Parameters.Add("@id", System.Data.DbType.Int32).Value = patient.ID;
            cmd.ExecuteNonQuery();
            logger.Info(string.Format("Deleted patient - {0}", patient));
        }

        public Study GetStudy(int id)
        {
            SQLiteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM studies WHERE id = @id";
            cmd.Parameters.Add("@id", System.Data.DbType.Int32).Value = id;
            SQLiteDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                logger.Info(string.Format("No study with id - {0}", id));
                return null;
            }
            else
            {
                if (reader.Read())
                {
                    Study s = new Study();
                    s.ID = id;
                    s.Date = DateTime.Parse(reader["studydate"].ToString());
                    s.PatientID = int.Parse(reader["patient_id"].ToString());
                    reader.Close();
                    logger.Info(string.Format("Got study - {0}", s));
                    return s;
                }
            }
            return null;
        }

        public List<Study> GetStudies()
        {
            List<Study> studies = new List<Study>();
            SQLiteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM studies";
            SQLiteDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                return studies;
            }
            else
            {
                while (reader.Read())
                {
                    Study s = new Study();
                    s.ID = int.Parse(reader["id"].ToString());
                    s.Date = DateTime.Parse(reader["studydate"].ToString());
                    s.PatientID = int.Parse(reader["patient_id"].ToString());
                    studies.Add(s);
                }
            }
            logger.Info(string.Format("Read {0} studies", studies.Count));
            return studies;
        }

        public List<Study> GetStudiesByPatient(Patient patient)
        {
            List<Study> studies = new List<Study>();
            SQLiteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM studies WHERE patient_id = @id";
            cmd.Parameters.Add("@id", System.Data.DbType.Int32).Value = patient.ID;
            SQLiteDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                return studies;
            }
            else
            {
                while (reader.Read())
                {
                    Study s = new Study();
                    s.ID = int.Parse(reader["id"].ToString());
                    s.Date = DateTime.Parse(reader["studydate"].ToString());
                    s.PatientID = int.Parse(reader["patient_id"].ToString());
                    studies.Add(s);
                }
            }
            logger.Info(string.Format("Read {0} studies for patient {1}", studies.Count, patient));
            return studies;
        }

        public void InsertStudy(Study study)
        {
            SQLiteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO studies (studydate, patient_id) values (@date, @patient_id)";
            cmd.Parameters.Add("@date", System.Data.DbType.Date).Value = study.Date;
            cmd.Parameters.Add("@patient_id", System.Data.DbType.Int32).Value = study.PatientID;
            cmd.ExecuteNonQuery();
            logger.Info(string.Format("Inserted study", study));
        }

        public void UpdateStudy(Study study)
        {
            SQLiteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "UPDATE studies SET studydate = @date, patient_id = @patient_id WHERE id = @id";
            cmd.Parameters.Add("@date", System.Data.DbType.String).Value = study.Date;
            cmd.Parameters.Add("@patient_id", System.Data.DbType.Int32).Value = study.PatientID;
            cmd.Parameters.Add("@id", System.Data.DbType.Int32).Value = study.ID;
            cmd.ExecuteNonQuery();
            logger.Info(string.Format("Updated study", study));
        }

        public void DeleteStudy(Study study)
        {
            SQLiteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM studies WHERE id = @id";
            cmd.Parameters.Add("@id", System.Data.DbType.Int32).Value = study.ID;
            cmd.ExecuteNonQuery();
            logger.Info(string.Format("Deleted study", study));
        }

        public Image GetImage(int id)
        {
            SQLiteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM images WHERE id = @id";
            cmd.Parameters.Add("@id", System.Data.DbType.Int32).Value = id;
            SQLiteDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                return null;
            }
            else
            {
                if (reader.Read())
                {
                    Image i = new Image();
                    i.ID = int.Parse(reader["id"].ToString());
                    i.Hash = reader["hash"].ToString();
                    i.StudyID = int.Parse(reader["study_id"].ToString());
                    i.State = int.Parse(reader["state"].ToString());
                    return i;
                }
            }
            return null;
        }


        public List<Image> GetImages()
        {
            List<Image> images = new List<Image>();
            SQLiteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM images";
            SQLiteDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                return images;
            }
            else
            {
                while (reader.Read())
                {
                    Image i = new Image();
                    i.ID = int.Parse(reader["id"].ToString());
                    i.Hash = reader["hash"].ToString();
                    i.StudyID = int.Parse(reader["study_id"].ToString());
                    i.State = int.Parse(reader["state"].ToString());
                    images.Add(i);
                }
            }
            logger.Info(string.Format("Read {0} images", images.Count));
            return images;
        }

        public List<Image> GetImagesByStudy(Study study)
        {
            List<Image> images = new List<Image>();
            SQLiteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM images WHERE study_id = @id";
            cmd.Parameters.Add("@id", System.Data.DbType.Int32).Value = study.ID;
            SQLiteDataReader reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                return images;
            }
            else
            {
                while (reader.Read())
                {
                    Image i = new Image();
                    i.ID = int.Parse(reader["id"].ToString());
                    i.Hash = reader["hash"].ToString();
                    i.State = int.Parse(reader["state"].ToString());
                    i.StudyID = int.Parse(reader["study_id"].ToString());
                    images.Add(i);
                }
            }
            logger.Info(string.Format("Read {0} images for study - {1}", images.Count, study));
            return images;
        }

        public void InsertImage(Image image)
        {
            SQLiteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO images (hash, study_id, state) values (@hash, @study_id, @state)";
            cmd.Parameters.Add("@hash", System.Data.DbType.String).Value = image.Hash;
            cmd.Parameters.Add("@study_id", System.Data.DbType.Int32).Value = image.StudyID;
            cmd.Parameters.Add("@state", System.Data.DbType.Int32).Value = image.State;
            cmd.ExecuteNonQuery();
            logger.Info(string.Format("Inserted image {0}", image));
        }

        public void UpdateImage(Image image)
        {
            SQLiteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "UPDATE images SET hash = @hash, study_id = @study_id, state = @state WHERE id = @id";
            cmd.Parameters.Add("@hash", System.Data.DbType.String).Value = image.Hash;
            cmd.Parameters.Add("@study_id", System.Data.DbType.Int32).Value = image.StudyID;
            cmd.Parameters.Add("@state", System.Data.DbType.Int32).Value = image.State;
            cmd.Parameters.Add("@id", System.Data.DbType.Int32).Value = image.ID;
            cmd.ExecuteNonQuery();
            logger.Info(string.Format("Updated image {0}", image));
        }

        public void DeleteImage(Image image)
        {
            SQLiteCommand cmd = connection.CreateCommand();
            cmd.CommandText = "DELETE FROM images WHERE id = @id";
            cmd.Parameters.Add("@id", System.Data.DbType.Int32).Value = image.ID;
            cmd.ExecuteNonQuery();
            logger.Info(string.Format("Deleted image {0}", image));
        }

        public int GetLastInsertID() {
            string sql = "SELECT last_insert_rowid()";
            SQLiteCommand cmd = new SQLiteCommand(sql, connection);
            var id = cmd.ExecuteScalar();
            int lastID = int.Parse(id.ToString());
            logger.Info(string.Format("Last inserted id = {0}", lastID));
            return lastID;
        }

        public List<string> DeleteStudyComplete(Study study)
        {
            var imagesToDelete = new List<string>();

            logger.Info(string.Format("Completed delete of study = {0}", study));
            var images = GetImagesByStudy(study);
            foreach (var image in images)
            {
                imagesToDelete.Add(image.Hash);
                DeleteImage(image);
            }
            DeleteStudy(study);

            
            return imagesToDelete;
        }

        public List<string> DeletePatientComplete(Patient patient)
        {
            var imagesToDelete = new List<string>();
            logger.Info(string.Format("Completed delete of patient = {0}", patient));
            var studies = GetStudiesByPatient(patient);
            foreach (var study in studies)
            {
                imagesToDelete.AddRange(DeleteStudyComplete(study));
            }
            DeletePatient(patient);

            return imagesToDelete;
        }
    }
}
