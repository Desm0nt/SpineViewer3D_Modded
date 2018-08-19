namespace SpineLib.DB
{
    public class Patient
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public int Age { get; set; }

        public override string ToString()
        {
            return string.Format("id:{0} - name:{1},{2},{3}, age:{4}", ID, Surname, Name, Patronymic, Age);
        }
    }
}
