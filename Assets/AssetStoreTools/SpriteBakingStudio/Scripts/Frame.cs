namespace SBS
{
    public class Frame
    {
        public static Frame begin = new Frame(0, 0);

        public int number;
        public float time;

        public Frame(int number_, float time_)
        {
            number = number_;
            time = time_;
        }

        public override bool Equals(object obj)
        {
            Frame other = (Frame)obj;
            return number == other.number;
        }

        public override int GetHashCode()
        {
            return number;
        }
    }
}
