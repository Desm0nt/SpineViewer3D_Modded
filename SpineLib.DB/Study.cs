using System;

namespace SpineLib.DB
{
    public class Study
    {
        public int ID { get; set; }
        public DateTime Date { get; set; }
        public int PatientID { get; set; }

        public override string ToString()
        {
            return string.Format("id:{0} - date:{1} - patientid:{2}", ID, Date.ToShortDateString(), PatientID);
        }
    }
}
