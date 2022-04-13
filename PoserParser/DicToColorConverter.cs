using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace PoserParser
{
	public class DicToColorConverter : IValueConverter
	{
		public static Dictionary<int, string> ids = new Dictionary<int, string>();
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
            object ret = "";
            var x = value;
            //if (x.ToString().Contains("111"))
            //{
            //    return System.Windows.Media.Brushes.LightBlue;
            //}
            foreach (var item in ids)
            {
                string temp = item.Value;
                temp = temp.Replace("/", "");
                if (x.ToString() == temp)
                {
                    ret = Brushes.LightYellow;
                }
            }
            return ret;
            


            //if (int.TryParse(value.ToString(), out int id))
            //{
            //    switch (ids[id])
            //    {
            //        case "Added":
            //            return new SolidColorBrush(Colors.LightGreen);
            //        case "Deleted":
            //            return new SolidColorBrush(Colors.Red);
            //        default:
            //            //if(ids[id].Contains("/"))
            //            return Brushes.LightYellow;
            //    }
            //}
            //else
            //    return null;

            //else
            //{
            //    foreach (var item in ids)
            //    {
            //        string temp = item.Value;
            //        temp.Replace("/", "");
            //        if(value.ToString() == temp)
            //        {
            //            ret = Brushes.LightYellow;
            //        }
            //    }
            //    return ret;
            //}
            //if (value.ToString().Contains("/"))
            //{
            //    return Brushes.LightYellow;
            //}
            //else
            //{
            //    return Brushes.LightGreen;
            //}
        }

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
