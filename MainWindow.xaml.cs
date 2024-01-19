using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Linq;
using Color = System.Drawing.Color;
using Image = System.Windows.Controls.Image;
using Pen = System.Drawing.Pen;
using Application = System.Windows.Application;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using static modules_seats.Settes;
using System.Windows.Media.TextFormatting;

namespace modules_seats
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string? _pathParamsFiles;
        private int _func = 0;
        private int[,]? _matrix;
        private Bitmap? _bitmap;
        private List<Modules> _lib = new List<Modules>();
        private List<Seats> _seats = new List<Seats>();
        private string outText = "";

        public MainWindow()
        {
            InitializeComponent();

            MenuItem1.IsEnabled = false;
            MenuItem3.IsEnabled = false;
        }
        
        // Кнопка Solve Решение алгоритма
        private void MenuItem1_OnClick(object sender, RoutedEventArgs e)
        {
            Dictionary<int, Tuple<double, double>> dictPoints = new Dictionary<int, Tuple<double, double>>();
            
            Img.Children.Clear();
            var width = 900;
            var height = 900;
            
            var n = _seats.Count;
            
            _bitmap = new Bitmap(width, height);
            var graphics = Graphics.FromImage(_bitmap);
            graphics.Clear(Color.Green);

            int squareSize = Math.Min(width / 3, height / 3); 

            int count = 0;
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    int x = col * squareSize + squareSize / 2; 
                    int y = row * squareSize + squareSize / 2; 
                    
                    dictPoints.Add(_seats[count].N, new Tuple<double, double>(x, y));
                    count++;
                }
            }
            
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    if (_matrix![i, j] > 0)
                    {
                        var x = dictPoints[i + 1];
                        var y = dictPoints[j + 1];
                        PointF point1 = new PointF((float)x.Item1, (float)x.Item2);
                        PointF point2 = new PointF((float)y.Item1, (float)y.Item2);
            
                        graphics.DrawLine(Pens.Gold, point1, point2);
                        
                        var centerX = (point1.X + point2.X) / 2;
                        var centerY = (point1.Y + point2.Y) / 2;
                        Font font = new Font("Arial", 14, System.Drawing.FontStyle.Bold);
                        Brush brush = Brushes.Azure;
            
                        SizeF textSize = graphics.MeasureString((_matrix[i, j]).ToString(), font);
                        float textX = centerX - textSize.Width / 2;
                        float textY = centerY - textSize.Height / 2;
            
                        graphics.DrawString((_matrix[i, j]).ToString(), font, brush, textX, textY);
            
                    }
                }
            }
            
            foreach (var vir in dictPoints)
            {
                var x = vir.Value.Item1;
                var y = vir.Value.Item2;
                graphics.FillRectangle(System.Drawing.Brushes.Gray, (float)(x - 50), (float)(y - 50), 100, 100);
                var font = new Font("Times New Roman", 16);
                graphics.DrawString($"{vir.Key}:{_seats[vir.Key-1].Type}", font, System.Drawing.Brushes.Black, (float)x-50, (float)y-50);
            }

            int totalConnectionLength = 0;
            var textOut = "";
            int initialSeatIndex = _seats.FindIndex(s => !s.Used && s.Type == _lib[0].Type);
            if (initialSeatIndex != -1)
            {
                _lib[0].N = _seats[initialSeatIndex].N;
                _seats[initialSeatIndex].Used = true;
                textOut += $"В качестве первого библиотечного модуля взят: {_lib[0].Type}\n";
            }

            for (int i = 1; i < _lib.Count; i++)
            {
                var module = _lib[i];
                if (module.N != -1) continue; // Если уже установлено, пропускаем

                int closestSeatIndex = -1;
                int shortestDistance = int.MaxValue;
                foreach (var seat in _seats.Where(s => !s.Used && s.Type == module.Type))
                {
                    // Предполагаем, что функция DijkstraAlgorithm находит расстояние между узлами seat.N - 1 и _lib[0].N - 1
                    int distance = DijkstraAlgorithm(_matrix, seat.N - 1, _lib[0].N - 1);
                    if (distance < shortestDistance)
                    {
                        closestSeatIndex = _seats.IndexOf(seat);
                        shortestDistance = distance;
                    }
                }

                if (closestSeatIndex != -1)
                {
                    module.N = _seats[closestSeatIndex].N;
                    _seats[closestSeatIndex].Used = true;
                    // Добавляем полученное расстояние к общей длине соединения
                    totalConnectionLength += shortestDistance;
                }
            }

            foreach(var module in _lib)
            {
                if (module.N != -1)
                {
                    textOut += $"Модуль {module.Type} размещен!\n";
                }
                else
                {
                    textOut += $"Модуль {module.Type} пропущен!\n";
                }
              
            }

            var postedModules = new List<int>();

            foreach (var module in _lib.Where(s => s.N != -1))
            {
                postedModules.Add(module.N);
                var x = dictPoints[module.N].Item1;
                var y = dictPoints[module.N].Item2;
                graphics.FillRectangle(System.Drawing.Brushes.Gold, (float)(x - 30), (float)(y - 30), 60, 60);
                var font = new Font("Times New Roman", 16);
                graphics.DrawString($"{module.Number}:{module.Type}", font, System.Drawing.Brushes.Black, (float)x - 30, (float)y - 30);
            }

            textOut += $"Суммарная длина соединений: {totalConnectionLength}";
            MessageBox.Show(textOut);
            outText = textOut;

            var image = new Image { Source = Settes.ConvertImg(_bitmap) };
            Img.Children.Add(image);

            MenuItem3.IsEnabled = true;
        }
        
      
        // Загрузка файлов
        private void MenuItem2_OnClick(object sender, RoutedEventArgs e)
        {
            _lib.Clear();
            _seats.Clear();
            
            var openDialogChoiceFolder = new System.Windows.Forms.FolderBrowserDialog();
            openDialogChoiceFolder.ShowDialog();
            _pathParamsFiles = openDialogChoiceFolder.SelectedPath;
            
            try
            {
                
                var arrayFile = File.ReadAllLines(_pathParamsFiles+@"/lib.txt", System.Text.Encoding.Default);
                int c = 1;
                foreach (var t in arrayFile)
                {
                    _lib.Add(new Modules() {Type = Convert.ToString(t), Number = c});
                    c++;
                }
                
                arrayFile = File.ReadAllLines(_pathParamsFiles+@"/seats.txt", System.Text.Encoding.Default);
                
                for (var i = 0; i < arrayFile.Length; i++)
                {
                    _seats.Add(new Seats() {Type = Convert.ToString(arrayFile[i]), N = i+1, Used = false});
                }
                _seats = _seats.OrderBy(module => module.N).ToList();
                
                arrayFile= File.ReadAllLines(_pathParamsFiles+@"/matrix.txt", System.Text.Encoding.Default);
                _matrix = new int[arrayFile.Length, arrayFile.Length];
                for (var i = 0; i < arrayFile.Length; i++)
                {
                    for (var j = 0; j < arrayFile[0].Split(' ').Length; j++) _matrix[i, j] = int.Parse(arrayFile[i].Split(' ')[j]);
                }
                MenuItem1.IsEnabled = true;
            }
            catch (Exception exception)
            {
                System.Windows.MessageBox.Show($"Error!\n{exception}");
            }
            MenuItem3.IsEnabled = false;
        }

        // Кнопка Save сохранение данных
        private void MenuItem3_OnClick(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Title = "Сохранить картинку как...";
            sfd.OverwritePrompt = true;                     // показывать ли "Перезаписать файл" если пользователь указывает имя файла, который уже существует
            sfd.CheckPathExists = true;                     // отображает ли диалоговое окно предупреждение, если пользователь указывает путь, который не существует
            sfd.Filter = "Image Files(*.BMP)|*.BMP|Image Files(*.JPG)|*.JPG|Image Files(*.GIF)|*.GIF|Image Files(*.PNG)|*.PNG|All files (*.*)|*.*";
            sfd.ShowHelp = true;                            // отображается ли кнопка Справка в диалоговом окне
            sfd.ShowDialog();

            using (FileStream fileStream = new FileStream(_pathParamsFiles + "/out.txt", FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(fileStream))
                {
                    writer.Write(outText);
                }
            }

            try
            {
                _bitmap?.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Png);
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("You click on cancel!\nOr error save file!");
            }
        }

        // Exit выход
        private void MenuItem4_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}