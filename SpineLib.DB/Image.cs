namespace SpineLib.DB
{
    public class Image
    {
        public int ID { get; set; }
        public string Hash { get; set; }
        public int StudyID { get; set; }
        public int State { get; set; }

        public override string ToString()
        {
            return string.Format("id:{0} - hash:{1} - studyid:{2} - state:{3}", ID, Hash, StudyID, State);
        }
    }
}
