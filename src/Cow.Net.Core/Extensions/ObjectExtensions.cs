namespace Cow.Net.Core.Extensions
{
    public static class ObjectExtensions
    {
        public static double ToDouble(this object o)
        {
            if (o == null)
                return double.NaN;

            double d;
            double.TryParse(o.ToString(), out d);
            return d;
        }

        public static int ToInt(this object o)
        {
            if (o == null)
                return 0;

            int i;
            int.TryParse(o.ToString(), out i);
            return i;
        }
    }
}
