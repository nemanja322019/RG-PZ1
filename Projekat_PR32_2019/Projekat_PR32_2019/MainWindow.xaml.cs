using Projekat_PR32_2019.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using static System.Windows.Forms.LinkLabel;
using Point = Projekat_PR32_2019.Model.Point;

namespace Projekat_PR32_2019
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
		public string fajlZaUcitavanje = "";
		public string putanja = "";

		public double noviX, noviY, xMin,xMax,yMin,yMax;
        private double X;
        private double Y;
        private List<SubstationEntity> substations;
		private List<NodeEntity> nodes;
		private List<SwitchEntity> switches;
		private List<LineEntity> lines;

        public List<Ellipse> listaElipsi = new List<Ellipse>();
        public List<Ellipse> substationElipse = new List<Ellipse>();
        public List<Ellipse> nodeElipse = new List<Ellipse>();
        public List<Ellipse> switchElipse = new List<Ellipse>();

        public List<Line> listaSvihVodovaZaBrisanje = new List<Line>();
        public List<Ellipse> preseci = new List<Ellipse>();
		public List<Ellipse> cvoroviZaBrisanje = new List<Ellipse>();

		public bool isEllipse;
		public Grid lastElement;

        public Dictionary<long, Tuple<int, int>> points = new Dictionary<long, Tuple<int, int>>();
		public List<LinijaNaGridu> SveLinije = new List<LinijaNaGridu>();

		public static readonly int velicina = 500;
		public TackaNaGridu[,] velicinaGrida = new TackaNaGridu[velicina, velicina];
		public int[,] poseceneTacke = new int[velicina, velicina];

		public List<System.Windows.Point> PointsList; //poligon

		public List<object> History;
		public int UndoRedoPosition;

        #region MainWindow
        public MainWindow()
        {
            InitializeComponent();

			substations = new List<SubstationEntity>();
			nodes = new List<NodeEntity>();
			switches = new List<SwitchEntity>();
			lines = new List<LineEntity>();

			//points = new List<Point>();

			PointsList = new List<System.Windows.Point>();

			History = new List<object>();
			UndoRedoPosition = -1;

			for (int i = 0; i < velicina; i++) // inicijalizacija grida --> VELICINA
			{
				for (int j = 0; j < velicina; j++)
				{
					velicinaGrida[i, j] = new TackaNaGridu();
				}
			}
		}
        #endregion

        #region Browse File
        private void Browse_Button_Click(object sender, RoutedEventArgs e)
		{
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            DialogResult result = folderDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string selectedPath = folderDialog.SelectedPath;
                if (!cmbPutanje.Items.Contains(selectedPath))
                {
                    cmbPutanje.Items.Add(selectedPath);
                    cmbPutanje.SelectedItem = selectedPath; 
                }
            }
        }

        private void cmbPutanje_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cmbFajlovi.Items.Clear();

            string selectedFolderPath = cmbPutanje.SelectedItem as string;

            if (selectedFolderPath != null)
            {
                try
                {
                    // Get all XML files in the selected folder
                    string[] xmlFiles = Directory.GetFiles(selectedFolderPath, "*.xml");

                    // Add XML file names to cmbFajlovi
                    foreach (string xmlFilePath in xmlFiles)
                    {
                        string xmlFileName = System.IO.Path.GetFileName(xmlFilePath);
                        cmbFajlovi.Items.Add(xmlFileName);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"An error occurred: {ex.Message}");
                }
            }
        }
        #endregion

        #region Load Button
        private void Load_Button_Click(object sender, RoutedEventArgs e)
        {
            //Load_button.IsEnabled = false;

            if (cmbPutanje.SelectedItem != null && cmbFajlovi.SelectedItem != null)
            {
				ClearData();
                string selectedFolderPath = cmbPutanje.SelectedItem.ToString();
                string selectedFileName = cmbFajlovi.SelectedItem.ToString();
                string completePath = System.IO.Path.Combine(selectedFolderPath, selectedFileName);

                ReadXML(completePath);

                FindScale();
                Draw();
                DrawLine();
            }
            else
            {
                System.Windows.MessageBox.Show("Please select a folder and an XML file.");
            }
        }

		public void ClearData()
		{
			canvas.Children.Clear();
            substations = new List<SubstationEntity>();
            nodes = new List<NodeEntity>();
            switches = new List<SwitchEntity>();
            lines = new List<LineEntity>();

            listaElipsi.Clear();
            substationElipse.Clear();
            nodeElipse.Clear();
            switchElipse.Clear();

            listaSvihVodovaZaBrisanje.Clear();
            preseci.Clear();
            cvoroviZaBrisanje.Clear();

            points.Clear();
            SveLinije.Clear();

            History = new List<object>();
            UndoRedoPosition = -1;

			poseceneTacke = new int[velicina, velicina];
            for (int i = 0; i < velicina; i++) // inicijalizacija grida --> VELICINA
            {
                for (int j = 0; j < velicina; j++)
                {
                    velicinaGrida[i, j] = new TackaNaGridu();
                }
            }

			lastElement = null;
			isEllipse = false;
        }
        #endregion

        #region Read XML
        private void ReadXML(string path)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);

            XmlNodeList nodeList;

            
            nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Substations/SubstationEntity");
            foreach (XmlNode node in nodeList)
            {
				SubstationEntity sub = new SubstationEntity();
                sub.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
                sub.Name = node.SelectSingleNode("Name").InnerText;
                sub.X = double.Parse(node.SelectSingleNode("X").InnerText, CultureInfo.InvariantCulture);
                sub.Y = double.Parse(node.SelectSingleNode("Y").InnerText, CultureInfo.InvariantCulture);
				sub.ToolTip = "Substation\nID: " + sub.Id + "  Name: " + sub.Name;
				sub.Color = Brushes.Red;

				ToLatLon(sub.X, sub.Y, 34, out noviX, out noviY);

				sub.X = noviX;
				sub.Y = noviY;

				substations.Add(sub);
			}

			
			nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Nodes/NodeEntity");
			foreach (XmlNode node in nodeList)
			{
				NodeEntity nodeobj = new NodeEntity();
				nodeobj.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
				nodeobj.Name = node.SelectSingleNode("Name").InnerText;
				nodeobj.X = double.Parse(node.SelectSingleNode("X").InnerText, CultureInfo.InvariantCulture);
				nodeobj.Y = double.Parse(node.SelectSingleNode("Y").InnerText, CultureInfo.InvariantCulture);
				nodeobj.ToolTip = "Node\nID: " + nodeobj.Id + "  Name: " + nodeobj.Name;
				nodeobj.Color = Brushes.Blue;

				ToLatLon(nodeobj.X, nodeobj.Y, 34, out noviX, out noviY);

				nodeobj.X = noviX;
				nodeobj.Y = noviY;

				nodes.Add(nodeobj);
			}

			
			nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Switches/SwitchEntity");
			foreach (XmlNode node in nodeList)
			{
				SwitchEntity switchobj = new SwitchEntity();
				switchobj.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
				switchobj.Name = node.SelectSingleNode("Name").InnerText;
				switchobj.X = double.Parse(node.SelectSingleNode("X").InnerText, CultureInfo.InvariantCulture);
				switchobj.Y = double.Parse(node.SelectSingleNode("Y").InnerText, CultureInfo.InvariantCulture); ;
				switchobj.Color = Brushes.Green;
				switchobj.Status = node.SelectSingleNode("Status").InnerText;
                switchobj.ToolTip = "Switch\nID: " + switchobj.Id + "  Name: " + switchobj.Name + "\nStatus: " + switchobj.Status;

                ToLatLon(switchobj.X, switchobj.Y, 34, out noviX, out noviY);

				switchobj.X = noviX;
				switchobj.Y = noviY;

				switches.Add(switchobj);
			}

			
			nodeList = xmlDoc.DocumentElement.SelectNodes("/NetworkModel/Lines/LineEntity");
			foreach (XmlNode node in nodeList)
			{
				LineEntity l = new LineEntity();
				l.Id = long.Parse(node.SelectSingleNode("Id").InnerText);
				l.Name = node.SelectSingleNode("Name").InnerText;
				if (node.SelectSingleNode("IsUnderground").InnerText.Equals("true"))
				{
					l.IsUnderground = true;
				}
				else
				{
					l.IsUnderground = false;
				}
				l.R = float.Parse(node.SelectSingleNode("R").InnerText);
				l.ConductorMaterial = node.SelectSingleNode("ConductorMaterial").InnerText;
				l.LineType = node.SelectSingleNode("LineType").InnerText;
				l.ThermalConstantHeat = long.Parse(node.SelectSingleNode("ThermalConstantHeat").InnerText);
				l.FirstEnd = long.Parse(node.SelectSingleNode("FirstEnd").InnerText);
				l.SecondEnd = long.Parse(node.SelectSingleNode("SecondEnd").InnerText);
				//Console.WriteLine("Pocetak: " + l.FirstEnd + " Kraj: " +l.SecondEnd);

				bool istaLinija = lines.Any(line => (l.FirstEnd == line.SecondEnd && l.SecondEnd == line.FirstEnd) || (l.FirstEnd == line.FirstEnd && l.FirstEnd == line.SecondEnd));
				if(!istaLinija)
                {
					lines.Add(l);
                }
			}

		}
        #endregion

        #region FindScale
        private void FindScale()
        {
			if (substations.Count > 0 && nodes.Count > 0 && switches.Count > 0)
			{
				xMax = Math.Max(Math.Max(substations.Max(item => item.X), nodes.Max(item => item.X)), switches.Max(item => item.X)) + 0.01;
				xMin = Math.Min(Math.Min(substations.Min(item => item.X), nodes.Min(item => item.X)), switches.Min(item => item.X)) - 0.01;
				X = (xMax - xMin) / velicina;
				yMax = Math.Max(Math.Max(substations.Max(item => item.Y), nodes.Max(item => item.Y)), switches.Max(item => item.Y)) + 0.01;
				yMin = Math.Min(Math.Min(substations.Min(item => item.Y), nodes.Min(item => item.Y)), switches.Min(item => item.Y)) - 0.01;
				Y = (yMax - yMin) / velicina;
			}
			else if(substations.Count > 0 && nodes.Count > 0)
			{
                xMax = Math.Max(substations.Max(item => item.X), nodes.Max(item => item.X)) + 0.01;
                xMin = Math.Min(substations.Min(item => item.X), nodes.Min(item => item.X)) - 0.01;
                X = (xMax - xMin) / velicina;
                yMax = Math.Max(substations.Max(item => item.Y), nodes.Max(item => item.Y)) + 0.01;
                yMin = Math.Min(substations.Min(item => item.Y), nodes.Min(item => item.Y)) - 0.01;
                Y = (yMax - yMin) / velicina;
            }
            else if (substations.Count > 0 && switches.Count > 0)
            {
                xMax = Math.Max(substations.Max(item => item.X), switches.Max(item => item.X)) + 0.01;
                xMin = Math.Min(substations.Min(item => item.X), switches.Min(item => item.X)) - 0.01;
                X = (xMax - xMin) / velicina;
                yMax = Math.Max(substations.Max(item => item.Y), switches.Max(item => item.Y)) + 0.01;
                yMin = Math.Min(substations.Min(item => item.Y), switches.Min(item => item.Y)) - 0.01;
                Y = (yMax - yMin) / velicina;
            }
            else if (nodes.Count > 0 && switches.Count > 0)
            {
                xMax = Math.Max(nodes.Max(item => item.X),switches.Max(item => item.X)) + 0.01;
                xMin = Math.Min(nodes.Min(item => item.X), switches.Min(item => item.X)) - 0.01;
                X = (xMax - xMin) / velicina;
                yMax = Math.Max(nodes.Max(item => item.Y), switches.Max(item => item.Y)) + 0.01;
                yMin = Math.Min(nodes.Min(item => item.Y), switches.Min(item => item.Y)) - 0.01;
                Y = (yMax - yMin) / velicina;
            }
            else if (substations.Count > 0)
            {
                xMax = substations.Max(item => item.X) + 0.01;
                xMin = substations.Min(item => item.X) - 0.01;
                X = (xMax - xMin) / velicina;
                yMax = substations.Max(item => item.Y) + 0.01;
                yMin = substations.Min(item => item.Y) - 0.01;
                Y = (yMax - yMin) / velicina;
            }
            else if (nodes.Count > 0)
            {
                xMax = nodes.Max(item => item.X) + 0.01;
                xMin = nodes.Min(item => item.X) - 0.01;
                X = (xMax - xMin) / velicina;
                yMax = nodes.Max(item => item.Y) + 0.01;
                yMin = nodes.Min(item => item.Y) - 0.01;
                Y = (yMax - yMin) / velicina;
            }
            else if (switches.Count > 0)
            {
                xMax = switches.Max(item => item.X) + 0.01;
                xMin = switches.Min(item => item.X) - 0.01;
                X = (xMax - xMin) / velicina;
                yMax = switches.Max(item => item.Y) +0.01;
                yMin = switches.Min(item => item.Y) -0.01;
                Y = (yMax - yMin) / velicina;
            }
        }
        #endregion

        #region Draw Elements
        private void Draw()
        {
			if(substations.Count > 0)
            {
				foreach(var substation in substations)
                {
                    var v = DrawElement(substation.X,substation.Y,substation.Id,substation.Name,substation.Color,"substation",substation.ToolTip);
                    v.SetValue(FrameworkElement.TagProperty, substation);
                }
            }
			if (nodes.Count > 0)
			{
				foreach (var node in nodes)
				{
                    var v = DrawElement(node.X, node.Y, node.Id, node.Name, node.Color,"node",node.ToolTip);
                    v.SetValue(FrameworkElement.TagProperty, node);
                }
			}
			if (switches.Count > 0)
			{
				foreach (var sw in switches)
				{
					var v = DrawElement(sw.X, sw.Y, sw.Id, sw.Name, sw.Color,"switch", sw.ToolTip);
                    v.SetValue(FrameworkElement.TagProperty, sw);
                }
			}
		}


		private Ellipse DrawElement(double xValue, double yValue, long id, string name, Brush color,string type, string tooltip)
        {

			double tmpX = xMax - xValue;
			double tmpY = yMax - yValue;
			int scaledX = (int)(tmpX / X);
			int scaledY = (int)(tmpY / Y);


			Ellipse el = new Ellipse();
			el.Stroke = color;
			el.Fill = color;
			el.Width = 2;
			el.Height = 2;
			el.ToolTip = tooltip;
			el.Name = String.Format("n" + id.ToString());
			if (!velicinaGrida[scaledY, scaledX].SadrziElement)
			{
				velicinaGrida[scaledY, scaledX].SadrziElement = true;
				Canvas.SetLeft(el, scaledY * 2 - 1);
				Canvas.SetTop(el, scaledX * 2 - 1);
                //Console.WriteLine("X: "+scaledY + " Y: "+scaledX);
				if(type is "substation")
				{
					substationElipse.Add(el);
				}
				else if(type is "node")
				{
					nodeElipse.Add(el);
				}
				else
				{
					switchElipse.Add(el);
				}
				listaElipsi.Add(el);
				points.Add(id,new Tuple<int,int>(scaledY, scaledX));

				return el;
			}
            else
            {
				int brojac = 1;
				while (true)
				{
					if (scaledY - brojac >= 0 && !velicinaGrida[scaledY - brojac, scaledX].SadrziElement) { scaledY -= brojac; break; }
					else if (scaledY + brojac <= 500 && !velicinaGrida[scaledY + brojac, scaledX].SadrziElement) { scaledY += brojac; break; }
					else if (scaledX - brojac >= 0 && !velicinaGrida[scaledY, scaledX - brojac].SadrziElement) { scaledX -= brojac; break; }
					else if (scaledX + brojac <= 500 && !velicinaGrida[scaledY, scaledX + brojac].SadrziElement) { scaledX += brojac; break; }
					else { brojac++; }
				}
				velicinaGrida[scaledY, scaledX].SadrziElement = true;

				Canvas.SetLeft(el, scaledY * 2 - 1);
				Canvas.SetTop(el, scaledX * 2 - 1);
				if(type is "substation")
				{
					substationElipse.Add(el);
				}
				else if(type is "node")
				{
					nodeElipse.Add(el);
				}
				else
				{
					switchElipse.Add(el);
				}
				listaElipsi.Add(el);
				points.Add(id, new Tuple<int, int>(scaledY, scaledX));

				return el;
			}
        }

        #endregion

        #region Draw Lines
        private void DrawLine()
        {
            foreach (LineEntity lineEntity in lines)
            {
                //da li u recniku ne postoji lineentity prvi ili drugi onda nastavim dalje u foreach
                if (!points.ContainsKey(lineEntity.FirstEnd) || !points.ContainsKey(lineEntity.SecondEnd))
                {
                    continue;
                }
                PozicijaUMatrici pocetnaPozicija = new PozicijaUMatrici()
                {//red,kolona i roditelj
                    Red = points[lineEntity.FirstEnd].Item1,
                    Kolona = points[lineEntity.FirstEnd].Item2,
                    Roditelj = null
                };
                long krajnjiID = lineEntity.SecondEnd;
                long pocetniID = lineEntity.FirstEnd;
                //pocetna pozicija za iscrtavanje
                pocetnaPozicija = BreadthFirstSearch(pocetnaPozicija, krajnjiID);//ovde mi se trenutno nalazi krajna pozicija


                while (pocetnaPozicija != null)
                {
                    if (pocetnaPozicija.Roditelj == null) //znaci da sam na 1. cvoru
                        break;
                    //koordinate za 1. objekat
                    int x1 = pocetnaPozicija.Red * 2;
                    int y1 = pocetnaPozicija.Kolona * 2;
                    //koordinate za 2. objekat
                    int x2 = pocetnaPozicija.Roditelj.Red * 2;
                    int y2 = pocetnaPozicija.Roditelj.Kolona * 2;
                    //da li postoji linija
                    bool postojiLinija = false;
                    foreach (LinijaNaGridu linijaNaGridu in SveLinije) //proveravam da li postoji vec nacrtana linija, iskoci ako postoji
                    {
                        if (linijaNaGridu.X1 == x1 && linijaNaGridu.Y1 == y1 && linijaNaGridu.X2 == x2 && linijaNaGridu.Y2 == y2) { postojiLinija = true; break; }
                        if (linijaNaGridu.X1 == x2 && linijaNaGridu.Y1 == y2 && linijaNaGridu.X2 == x1 && linijaNaGridu.Y2 == y1) { postojiLinija = true; break; }
                    }
                    if (!postojiLinija)//ako ne postoji
                    {
                        Line novaLinija = new Line()//nacrtaj liniju izmedju datih koordinata
                        {
                            Stroke = Brushes.Black,
                            StrokeThickness = 0.8,
                            ToolTip = String.Format("ID: {0} \nName: {1}", lineEntity.Id, lineEntity.Name),
                            X1 = x1,
                            X2 = x2,
                            Y1 = y1,
                            Y2 = y2,
                        };


                        //Desnim klikom na vod između dva entiteta treba ponuditi opciju da entiteti povezani tim vodom
                        //budu obojeni različitim bojama od ostalih kako bi korisnik znao koji su entiteti povezani tim vodom

                        novaLinija.MouseRightButtonDown += LineAnimation;
						SveLinije.Add(new LinijaNaGridu(novaLinija.X1, novaLinija.Y1, novaLinija.X2, novaLinija.Y2, pocetniID, krajnjiID));
						novaLinija.SetValue(TagProperty, lineEntity);
                        canvas.Children.Add(novaLinija);
                        x1 = pocetnaPozicija.Kolona;
                        x2 = pocetnaPozicija.Roditelj.Kolona;
                        y1 = pocetnaPozicija.Red;
                        y2 = pocetnaPozicija.Roditelj.Red;

                        //uslovi da li se presecaju 2 linije
                        // napravi elipsu na preseku linija da izgleda kao cvor
                        if (!velicinaGrida[y1, x1].SadrziElement && !velicinaGrida[y1, x1].Posecen) { velicinaGrida[y1, x1].Posecen = true; velicinaGrida[y1, x1].PosecenId.Add(lineEntity.Id); }
                        else if (!velicinaGrida[y1, x1].SadrziElement && velicinaGrida[y1, x1].Posecen && !velicinaGrida[y1, x1].PosecenId.Contains(lineEntity.Id)) { NapraviOblik(lineEntity.Id, x1, y1); }

                        if (!velicinaGrida[y2, x2].SadrziElement && !velicinaGrida[y2, x2].Posecen) { velicinaGrida[y2, x2].Posecen = true; velicinaGrida[y2, x2].PosecenId.Add(lineEntity.Id); }
                        else if (!velicinaGrida[y2, x2].SadrziElement && velicinaGrida[y2, x2].Posecen && !velicinaGrida[y2, x2].PosecenId.Contains(lineEntity.Id)) { NapraviOblik(lineEntity.Id, x2, y2); }
                    }
                    pocetnaPozicija = pocetnaPozicija.Roditelj;//stavljam roditelja od end node na pocetnu poziciju
                }

            }
            foreach (Ellipse elipsa in listaElipsi) //za svaku elipsu koju sam dodavao u metodama za DRAW 
            {
                canvas.Children.Add(elipsa); //dodaj je na Canvas, jer nisu prethodno dodate
                                             //  vec su u tim metodama samo punjene liste
            }
        }

        public void NapraviOblik(long lineId, int x, int y)
        {
            #region elipsa
            Ellipse ellipse = new Ellipse
            {
                ToolTip = String.Format("Cvor"),
                Fill = Brushes.Black,
                Width = 2,
                Height = 2
            };
            Canvas.SetTop(ellipse, x * 2 - 1);
            Canvas.SetLeft(ellipse, y * 2 - 1);
            #endregion

            canvas.Children.Add(ellipse);
            preseci.Add(ellipse);
            velicinaGrida[x, y].PosecenId.Add(lineId);
        }

        #endregion

        #region LineAnimation

        public string prethodniPocetniCvor = String.Empty, prethodniKrajnjiCvor = String.Empty;
		public Brush prethodnaBojaPocetka, prethodnaBojaKraja;

		public void LineAnimation(object sender, MouseButtonEventArgs e)
		{


			if (!prethodniPocetniCvor.Equals("") && !prethodniKrajnjiCvor.Equals(""))
			{
				foreach (var el in listaElipsi)
				{
					if (prethodniPocetniCvor.Equals(el.Name))
					{
						DoubleAnimation sizeAnimation = new DoubleAnimation();
						sizeAnimation.From = el.Width;
						sizeAnimation.To = 2;
						sizeAnimation.Duration = TimeSpan.FromSeconds(0);
						el.BeginAnimation(FrameworkElement.WidthProperty, sizeAnimation);

						sizeAnimation = new DoubleAnimation();
						sizeAnimation.From = el.Height;
						sizeAnimation.To = 2;
						sizeAnimation.Duration = TimeSpan.FromSeconds(0);
						el.BeginAnimation(FrameworkElement.HeightProperty, sizeAnimation);

						// Create a ColorAnimation object to animate the color
						ColorAnimation colorAnimation = new ColorAnimation();
						colorAnimation.From = Colors.Yellow;
						colorAnimation.To = ((SolidColorBrush)prethodnaBojaPocetka).Color;
						colorAnimation.Duration = TimeSpan.FromSeconds(0);

						// Create a new SolidColorBrush with the initial color and animate its color
						SolidColorBrush brush = (SolidColorBrush)el.Fill;
						SolidColorBrush animatedBrush = new SolidColorBrush(brush.Color);
						el.Fill = animatedBrush;
						el.Stroke = animatedBrush;
						animatedBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
					}
					else if (prethodniKrajnjiCvor.Equals(el.Name))
					{
						DoubleAnimation sizeAnimation = new DoubleAnimation();
						sizeAnimation.From = el.Width;
						sizeAnimation.To = 2;
						sizeAnimation.Duration = TimeSpan.FromSeconds(0);
						el.BeginAnimation(FrameworkElement.WidthProperty, sizeAnimation);

						sizeAnimation = new DoubleAnimation();
						sizeAnimation.From = el.Height;
						sizeAnimation.To = 2;
						sizeAnimation.Duration = TimeSpan.FromSeconds(0);
						el.BeginAnimation(FrameworkElement.HeightProperty, sizeAnimation);

						// Create a ColorAnimation object to animate the color
						ColorAnimation colorAnimation = new ColorAnimation();
						colorAnimation.From = Colors.Yellow;
						colorAnimation.To = ((SolidColorBrush)prethodnaBojaPocetka).Color;
						colorAnimation.Duration = TimeSpan.FromSeconds(0);

						// Create a new SolidColorBrush with the initial color and animate its color
						SolidColorBrush brush = (SolidColorBrush)el.Fill;
						SolidColorBrush animatedBrush = new SolidColorBrush(brush.Color);
						el.Fill = animatedBrush;
						el.Stroke = animatedBrush;
						animatedBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
					}
				}
			}
			Line selected = (Line)e.OriginalSource;
			string pocetnoImeCvora = String.Empty, krajnjeImeCvora = String.Empty;

			foreach (var linije in SveLinije)
			{
				if (linije.X1 == selected.X1 && linije.X2 == selected.X2 && linije.Y1 == selected.Y1 && linije.Y2 == selected.Y2)
				{
					pocetnoImeCvora = String.Format("n" + linije.PocetniCvor.ToString());
					krajnjeImeCvora = String.Format("n" + linije.KrajnjiCvor.ToString());
					break;
				}
			}

			foreach (var el in listaElipsi)
			{
				if (el.Name.Equals(pocetnoImeCvora))
				{
					prethodniPocetniCvor = pocetnoImeCvora;
					prethodnaBojaPocetka = el.Fill;



					// Create a DoubleAnimation object to animate the width and height properties
					DoubleAnimation sizeAnimation = new DoubleAnimation();
					sizeAnimation.From = el.Width;
					sizeAnimation.To = 4;
					sizeAnimation.Duration = TimeSpan.FromSeconds(1);
					el.BeginAnimation(FrameworkElement.WidthProperty, sizeAnimation);

					sizeAnimation = new DoubleAnimation();
					sizeAnimation.From = el.Height;
					sizeAnimation.To = 4;
					sizeAnimation.Duration = TimeSpan.FromSeconds(1);
					el.BeginAnimation(FrameworkElement.HeightProperty, sizeAnimation);

					// Create a ColorAnimation object to animate the color
					ColorAnimation colorAnimation = new ColorAnimation();
					colorAnimation.From = ((SolidColorBrush)el.Fill).Color;
					colorAnimation.To = Colors.Yellow;
					colorAnimation.Duration = TimeSpan.FromSeconds(0);

					// Create a new SolidColorBrush with the initial color and animate its color
					SolidColorBrush brush = (SolidColorBrush)el.Fill;
					SolidColorBrush animatedBrush = new SolidColorBrush(brush.Color);
					el.Fill = animatedBrush;
					el.Stroke = animatedBrush;
					animatedBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);


				}
				else if (el.Name.Equals(krajnjeImeCvora))
				{
					prethodniKrajnjiCvor = krajnjeImeCvora;
					prethodnaBojaKraja = el.Fill;

					// Create a DoubleAnimation object to animate the width and height properties
					DoubleAnimation sizeAnimation = new DoubleAnimation();
					sizeAnimation.From = el.Width;
					sizeAnimation.To = 4;
					sizeAnimation.Duration = TimeSpan.FromSeconds(1);
					el.BeginAnimation(FrameworkElement.WidthProperty, sizeAnimation);

					sizeAnimation = new DoubleAnimation();
					sizeAnimation.From = el.Height;
					sizeAnimation.To = 4;
					sizeAnimation.Duration = TimeSpan.FromSeconds(1);
					el.BeginAnimation(FrameworkElement.HeightProperty, sizeAnimation);

					// Create a ColorAnimation object to animate the color
					ColorAnimation colorAnimation = new ColorAnimation();
					colorAnimation.From = ((SolidColorBrush)el.Fill).Color;
					colorAnimation.To = Colors.Yellow;
					colorAnimation.Duration = TimeSpan.FromSeconds(0);

					// Create a new SolidColorBrush with the initial color and animate its color
					SolidColorBrush brush = (SolidColorBrush)el.Fill;
					SolidColorBrush animatedBrush = new SolidColorBrush(brush.Color);
					el.Fill = animatedBrush;
					el.Stroke = animatedBrush;
					animatedBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
				}
			}
		}
        #endregion 

        #region BFS
        private PozicijaUMatrici BreadthFirstSearch(PozicijaUMatrici pocetnaPozicija, long idKrajnjePozicije)
		{
			List<PozicijaUMatrici> pozicija = new List<PozicijaUMatrici>();
			pozicija.Add(pocetnaPozicija);
			while (true)
			{
				foreach (var poz in pozicija)
				{ //cvor je svaki element
					if (points[idKrajnjePozicije].Item1 == poz.Red && points[idKrajnjePozicije].Item2 == poz.Kolona) //da li postoji putanja od cvora a do cvora b
					{
						for (int i = 0; i < velicina; i++)
						{
							for (int j = 0; j < velicina; j++)
							{               //x,y
								poseceneTacke[i, j] = 0; //matrica dimenzija 500 * 500 sa nulama na svim pozicijama
														 //kako bih znao na kojoj poziciji sam bio stavljajuci 1 na tu poziciju
							}
						}
						return poz; //ovde vraca krajnju poziciju 
					}
				}
				List<PozicijaUMatrici> listaPozicijaUMatrici = new List<PozicijaUMatrici>();
				foreach (var poz in pozicija)
				{ //ispitamo sva ogranicenja
					if (poz.Red + 1 >= 0 && poz.Kolona >= 0 && poz.Red + 1 < velicina && poz.Kolona < velicina && poseceneTacke[poz.Red + 1, poz.Kolona] == 0)
					{
						poseceneTacke[poz.Red + 1, poz.Kolona] = 1; //stavljam 1 na mesto 0
						listaPozicijaUMatrici.Add(new PozicijaUMatrici() { Red = poz.Red + 1, Kolona = poz.Kolona, Roditelj = poz }); //dodam mu bas to sto sam ispitao i stavim da je roditelj cvor iz pozicije
					}
					if (poz.Red - 1 >= 0 && poz.Kolona >= 0 && poz.Red - 1 < velicina && poz.Kolona < velicina && poseceneTacke[poz.Red - 1, poz.Kolona] == 0)
					{
						poseceneTacke[poz.Red - 1, poz.Kolona] = 1;
						listaPozicijaUMatrici.Add(new PozicijaUMatrici() { Red = poz.Red - 1, Kolona = poz.Kolona, Roditelj = poz });
					}
					if (poz.Red >= 0 && poz.Kolona + 1 >= 0 && poz.Red < velicina && poz.Kolona + 1 < velicina && poseceneTacke[poz.Red, poz.Kolona + 1] == 0)
					{
						poseceneTacke[poz.Red, poz.Kolona + 1] = 1;
						listaPozicijaUMatrici.Add(new PozicijaUMatrici() { Red = poz.Red, Kolona = poz.Kolona + 1, Roditelj = poz });
					}
					if (poz.Red >= 0 && poz.Kolona - 1 >= 0 && poz.Red < velicina && poz.Kolona - 1 < velicina && poseceneTacke[poz.Red, poz.Kolona - 1] == 0)
					{
						poseceneTacke[poz.Red, poz.Kolona - 1] = 1;
						listaPozicijaUMatrici.Add(new PozicijaUMatrici() { Red = poz.Red, Kolona = poz.Kolona - 1, Roditelj = poz });
					}
				}
				pozicija = new List<PozicijaUMatrici>(listaPozicijaUMatrici);//ova lista je puna sad
			}
		}
        #endregion

        #region Mouse click
        private void mouseRightButtonDown_Canvas(object sender, MouseButtonEventArgs e)
		{
			System.Windows.Point point = e.GetPosition((IInputElement)sender);
			if ((bool)Ellipse_RadioButton.IsChecked)
			{
				DrawEllipseWindow drawElipseWindow = new DrawEllipseWindow(point, this);
				drawElipseWindow.Show();
            }
            else if ((bool)Polygon_RadioButton.IsChecked)
            {
                PointsList.Add(point);
            }
            else if ((bool)Text_RadioButton.IsChecked)
            {
                AddTextWindow textWindow = new AddTextWindow(point, this);
                textWindow.Show();
            }
        }

		private void mouseLeftButtonDown_Canvas(object sender, MouseButtonEventArgs e)
		{
			System.Windows.Point point = e.GetPosition((IInputElement)sender);
			if ((bool)Polygon_RadioButton.IsChecked && !(bool)Edit_RadioButton.IsChecked)
			{
				if (this.PointsList.Count >= 3)
				{
					DrawPolygonWindow drawPolygonWindow = new DrawPolygonWindow(this, PointsList);
					//pointsList.Clear();
					drawPolygonWindow.Show();
				}
				else
				{
					System.Windows.MessageBox.Show("You have to choose at least 3 points for polygon!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);

				}

			}
			else if(e.OriginalSource is Ellipse)
			{
				Ellipse selected = (Ellipse)e.OriginalSource;
				var switchEn = selected.GetValue(TagProperty);
				if (switchEn is SwitchEntity)
				{
					SwitchEntity swE = switchEn as SwitchEntity;

                    SwitchStatusWindow switchStatusWindow = new SwitchStatusWindow(swE, this, selected);
                    switchStatusWindow.Show();
                }
			}

		}
        #endregion

        #region CTRL + D Shortcut
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.D && Keyboard.Modifiers == ModifierKeys.Control)
            {
                System.Windows.Point mousePosition = Mouse.GetPosition(canvas); // Change 'Canvas' to your canvas name

				if (lastElement != null)
				{
                    Grid grid = new Grid();
                    if (isEllipse)
					{
						double height = 0, width = 0;
                        foreach (UIElement child in lastElement.Children)
                        {
							if (child is Ellipse)
							{
								height = ((Ellipse)child).ActualHeight;
								width = ((Ellipse)child).ActualWidth;
							}
							
							UIElement clonedChild = CloneUIElement(child);
							grid.Children.Add(clonedChild);
							
                        }
						foreach(UIElement child in grid.Children)
						{
                            if (child is Ellipse)
                            {
								Ellipse ellipse = (Ellipse)child;
								ellipse.Width = width;
								ellipse.Height = height;
								grid.Height = height;
								grid.Width = width;
                                grid.MouseLeftButtonDown += (esender, ee) => EditObjects.UpdateEllipse(esender, ee, this, grid, ellipse);
                            }
                        }
                        Canvas.SetLeft(grid, mousePosition.X);
                        Canvas.SetTop(grid, mousePosition.Y);
                        canvas.Children.Add(grid);
                    }
					else
					{

						foreach (UIElement child in lastElement.Children)
						{
							UIElement clonedChild = CloneUIElement(child);
							grid.Children.Add(clonedChild);
						}
                        foreach (UIElement child in grid.Children)
						{
							if(child is Polygon)
							{
								Polygon polygon = (Polygon)child;
                                polygon.MouseLeftButtonDown += (esender, ee) => EditObjects.UpdatePolygon(esender, ee, this, grid, polygon);
                            }
							if(child is TextBlock)
							{
								TextBlock textBlock = (TextBlock)child;
                                grid.MouseLeftButtonDown += (esender, ee) => EditObjects.UpdateText(esender, ee, this, grid, textBlock);
                            }
						}
                        Canvas.SetLeft(grid, mousePosition.X);
						Canvas.SetTop(grid, mousePosition.Y);
						canvas.Children.Add(grid);
					}

                    History.Add(grid);
                    UndoRedoPosition++;
                }
            }
        }

        private UIElement CloneUIElement(UIElement source)
        {
            if (source is FrameworkElement frameworkElement)
            {
                Type elementType = source.GetType();

                FrameworkElement clonedElement = (FrameworkElement)Activator.CreateInstance(elementType);

                foreach (PropertyInfo property in elementType.GetProperties())
                {
                    if (property.CanWrite)
                    {
                        object value = property.GetValue(frameworkElement);
                        property.SetValue(clonedElement, value);
                    }
                }

                return clonedElement;
            }

            return null;
        }
        #endregion

        #region UNDO REDO CLEAR
        private void button_Undo_Click(object sender, RoutedEventArgs e)
		{
			if (UndoRedoPosition > -1)
			{
				if (History[UndoRedoPosition] is Ellipse)
				{
					DependencyObject parent = VisualTreeHelper.GetParent((Ellipse)History[UndoRedoPosition]);
					this.canvas.Children.Remove((Grid)parent);
					UndoRedoPosition--;
				}
				else if (History[UndoRedoPosition] is Polygon)
				{
					DependencyObject parent = VisualTreeHelper.GetParent((Polygon)History[UndoRedoPosition]);
					this.canvas.Children.Remove((Grid)parent);
					UndoRedoPosition--;

				}
				else if (History[UndoRedoPosition] is TextBlock)
				{
					DependencyObject parent = VisualTreeHelper.GetParent((TextBlock)History[UndoRedoPosition]);
					this.canvas.Children.Remove((Grid)parent);
					UndoRedoPosition--;
				}
				else if (History[UndoRedoPosition] is Grid)
				{
					this.canvas.Children.Remove((Grid)History[UndoRedoPosition]);
                    UndoRedoPosition--;
                }
			}
            else
            {
				while (History.Count > UndoRedoPosition + 1)
				{
					button_Redo_Click(sender, e);
				}
			}
		}

		private void button_Redo_Click(object sender, RoutedEventArgs e)
		{
			if (History.Count > UndoRedoPosition + 1)
			{
				if (History[UndoRedoPosition + 1] is Ellipse)
				{
					DependencyObject parent = VisualTreeHelper.GetParent((Ellipse)History[UndoRedoPosition + 1]);
					this.canvas.Children.Add((Grid)parent);
					UndoRedoPosition++;
				}
				else if (History[UndoRedoPosition + 1] is Polygon)
				{
					DependencyObject parent = VisualTreeHelper.GetParent((Polygon)History[UndoRedoPosition + 1]);
					this.canvas.Children.Add((Grid)parent);
					UndoRedoPosition++;
				}
				else if (History[UndoRedoPosition + 1] is TextBlock)
				{
					DependencyObject parent = VisualTreeHelper.GetParent((TextBlock)History[UndoRedoPosition + 1]);
					this.canvas.Children.Add((Grid)parent);
					UndoRedoPosition++;
				}
                else if (History[UndoRedoPosition + 1] is Grid)
                {
                    this.canvas.Children.Add((Grid)History[UndoRedoPosition + 1]);
                    UndoRedoPosition++;
                }
            }
		}

		private void button_Clear_Click(object sender, RoutedEventArgs e)
		{
			foreach (var item in History)
			{
				if (item is Ellipse)
				{
					Ellipse ell = (Ellipse)item;
					DependencyObject parent = VisualTreeHelper.GetParent(ell);
					this.canvas.Children.Remove((Grid)parent);
				}
				else if (item is Polygon)
				{
					Polygon pol = (Polygon)item;
					DependencyObject parent = VisualTreeHelper.GetParent(pol);
					this.canvas.Children.Remove((Grid)parent);
				}
				else if (item is TextBlock)
				{
					TextBlock tb = (TextBlock)item;
					DependencyObject parent = VisualTreeHelper.GetParent(tb);
					this.canvas.Children.Remove((Grid)parent);
				}
                else if (item is Grid)
                {
                    this.canvas.Children.Remove((Grid)item);
                }
            }
			UndoRedoPosition = -1;
		}
        #endregion

        // PROJEKAT INTERAKCIJA

        private void button_SaveCanvasAsImage_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = @"C:\Users\Nex\Desktop\Grafika\Projekat 1\Projekat_PR32_2019\Projekat_PR32_2019\slike";
            string timestamp = DateTime.Now.ToString("dd.MM.yyyy. HH.mm.ss");
            string fileName = "canvas_image_" + timestamp + ".png";
            string fullPath = System.IO.Path.Combine(folderPath, fileName);

            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)canvas.Width, (int)canvas.Height, 96d, 96d, PixelFormats.Pbgra32);

            renderBitmap.Render(canvas);

            // Create a PngBitmapEncoder
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            // Save the image to the specified file path
            using (FileStream fileStream = new FileStream(fullPath, FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }

        private void HideInactive_Click(object sender, RoutedEventArgs e)
        {
			foreach (Shape v in canvas.Children)
			{
				var vod = v.GetValue(TagProperty);
				if(vod is LineEntity)
				{
                    LineEntity line = vod as LineEntity;

                    foreach (Shape v1 in canvas.Children)
                    {
						if (!(v1 is Line))
						{
							var switchEn = v1.GetValue(TagProperty);
							if (switchEn is SwitchEntity)
							{
								SwitchEntity swE = switchEn as SwitchEntity;
								if (swE.Status == "Open" && swE.Id == line.FirstEnd)
								{
									listaSvihVodovaZaBrisanje.Add((Line)v);

									foreach (Shape v2 in canvas.Children)
									{
										var entitet = v2.GetValue(TagProperty);
										if (entitet is NodeEntity)
										{
											NodeEntity temp = entitet as NodeEntity;
											if (temp.Id == line.SecondEnd)
											{
												cvoroviZaBrisanje.Add((Ellipse)v2);
												break;
											}
										}
										else if (entitet is SwitchEntity)
										{
											SwitchEntity temp = entitet as SwitchEntity;
											if (temp.Id == line.SecondEnd)
											{
												cvoroviZaBrisanje.Add((Ellipse)v2);
												break;
											}
										}
										else if (entitet is SubstationEntity)
										{
											SubstationEntity temp = entitet as SubstationEntity;
											if (temp.Id == line.SecondEnd)
											{
												cvoroviZaBrisanje.Add((Ellipse)v2);
												break;
											}
										}
									}
									break;
								}
							}
						}
                    }
                }
            }
            foreach (Line modelZaBrisanje in listaSvihVodovaZaBrisanje)
            {
                canvas.Children.Remove(modelZaBrisanje);
            }
			foreach(Ellipse ellipse in cvoroviZaBrisanje)
			{
				canvas.Children.Remove(ellipse);
			}
        }

        private void ShowInactive_Click(object sender, RoutedEventArgs e)
        {
            foreach (Line line in listaSvihVodovaZaBrisanje)
            {
                if (!canvas.Children.Contains(line))
                    canvas.Children.Add(line);
            }
			foreach(Ellipse ellipse in cvoroviZaBrisanje)
			{
				if(!canvas.Children.Contains(ellipse))
				{
					canvas.Children.Add(ellipse);
				}
			}
            listaSvihVodovaZaBrisanje.Clear();
			cvoroviZaBrisanje.Clear();
        }

        private void HideLines_Click(object sender, RoutedEventArgs e)
        {
			foreach(var v in canvas.Children)
			{
				if(v is Line)
				{
                    listaSvihVodovaZaBrisanje.Add((Line)v);
                }
			}
            foreach (Line line in listaSvihVodovaZaBrisanje)
			{
				canvas.Children.Remove(line);
			}
			foreach(Ellipse el in preseci)
			{
				canvas.Children.Remove(el);
			}
        }

        private void ShowLines_Click(object sender, RoutedEventArgs e)
        {
            foreach (Line line in listaSvihVodovaZaBrisanje)
            {
				if(!canvas.Children.Contains(line))
					canvas.Children.Add(line);
            }
            foreach (Ellipse el in preseci)
            {
                if (!canvas.Children.Contains(el))
                    canvas.Children.Add(el);
            }
			listaSvihVodovaZaBrisanje.Clear();
        }
        private void HideSubstations_Click(object sender, RoutedEventArgs e)
        {
            foreach (Ellipse elipsa in substationElipse) 
            {
				canvas.Children.Remove(elipsa);
            }
        }
        private void ShowSubstations_Click(object sender, RoutedEventArgs e)
        {
            foreach (Ellipse elipsa in substationElipse)
            {
                if (!canvas.Children.Contains(elipsa))
                    canvas.Children.Add(elipsa);
            }
        }

        private void HideNodes_Click(object sender, RoutedEventArgs e)
        {
            foreach (Ellipse elipsa in nodeElipse)
            {
                canvas.Children.Remove(elipsa);
            }
        }
        private void ShowNodes_Click(object sender, RoutedEventArgs e)
        {
            foreach (Ellipse elipsa in nodeElipse)
            {
                if (!canvas.Children.Contains(elipsa))
                    canvas.Children.Add(elipsa);
            }
        }

        private void HideSwitches_Click(object sender, RoutedEventArgs e)
        {
            foreach (Ellipse elipsa in switchElipse)
            {
                canvas.Children.Remove(elipsa);
            }
        }
        private void ShowSwitches_Click(object sender, RoutedEventArgs e)
        {
            foreach (Ellipse elipsa in switchElipse)
            {
                if (!canvas.Children.Contains(elipsa))
                    canvas.Children.Add(elipsa);
            }
        }

        #region ToLatLon
        public static void ToLatLon(double utmX, double utmY, int zoneUTM, out double latitude, out double longitude)
		{
			bool isNorthHemisphere = true;

			var diflat = -0.00066286966871111111111111111111111111;
			var diflon = -0.0003868060578;

			var zone = zoneUTM;
			var c_sa = 6378137.000000;
			var c_sb = 6356752.314245;
			var e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
			var e2cuadrada = Math.Pow(e2, 2);
			var c = Math.Pow(c_sa, 2) / c_sb;
			var x = utmX - 500000;
			var y = isNorthHemisphere ? utmY : utmY - 10000000;

			var s = ((zone * 6.0) - 183.0);
			var lat = y / (c_sa * 0.9996);
			var v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
			var a = x / v;
			var a1 = Math.Sin(2 * lat);
			var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
			var j2 = lat + (a1 / 2.0);
			var j4 = ((3 * j2) + a2) / 4.0;
			var j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
			var alfa = (3.0 / 4.0) * e2cuadrada;
			var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
			var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
			var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
			var b = (y - bm) / v;
			var epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
			var eps = a * (1 - (epsi / 3.0));
			var nab = (b * (1 - epsi)) + lat;
			var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
			var delt = Math.Atan(senoheps / (Math.Cos(nab)));
			var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

			longitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
			latitude = ((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
		}
        #endregion

    }
}
