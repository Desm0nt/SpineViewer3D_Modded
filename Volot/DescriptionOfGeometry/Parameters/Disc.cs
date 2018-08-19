using System.Collections.Generic;

namespace Volot.DescriptionOfGeometry.Parameters
{
    public class Disc
    {
        public string KeyUp { get; private set; } //Ключ верхнего позвонка 
        public string KeyDown { get; private set; } //Ключ нижнего позвонка 
        public List<Vertex> Model { get; private set; } //Точеная модель межпозвоночного диска

        public Disc(Spine s1, Spine s2)
        {
            //добавить проверку на верх-низ


            
        }
    }
}