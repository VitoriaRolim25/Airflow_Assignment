using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;


namespace Airflow_Assignment
{
    public class Main : IExternalApplication {
        public Result OnStartup(UIControlledApplication a) {
            a.CreateRibbonTab("BIMLOGIC");
            RibbonPanel painel = a.CreateRibbonPanel("BIMLOGIC", "Airflow");

            PushButtonData aplicarParametro = new PushButtonData("PARAMETRO", "Airflow",
            System.Reflection.Assembly.GetExecutingAssembly().Location, typeof(Commands).FullName);
            aplicarParametro.LargeImage = BitmapToBitmapImagConverter.ConvertToBitmapImage(Properties.Resources.Icone_Grande);
            aplicarParametro.Image = BitmapToBitmapImagConverter.ConvertToBitmapImage(Properties.Resources.Icone_Pequeno);
            painel.AddItem(aplicarParametro);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a) {
            return Result.Succeeded;
        }


    }
    public static class BitmapToBitmapImagConverter {
        public static BitmapImage ConvertToBitmapImage(Bitmap objBitmapSource) {
            BitmapImage objResult = null;
            using (MemoryStream objMemoryStream = new MemoryStream()) {
                objBitmapSource.Save(objMemoryStream, ImageFormat.Png);

                objResult = new BitmapImage();
                objResult.BeginInit();
                objResult.StreamSource = new MemoryStream(objMemoryStream.ToArray());
                objResult.EndInit();
            }

            return objResult;
        }
    }

}
