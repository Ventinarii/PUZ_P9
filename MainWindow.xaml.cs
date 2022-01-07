using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace P9
{
    /// <summary>
    /// this is simple simulation for problem 9.3
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// this class represents singe node that is under simulation
        /// </summary>
        public class Actor {
            public int Id { get; set; }
            /// <summary>
            /// what Actor is following
            /// </summary>
            public Actor Target { get; set; }
            /// <summary>
            /// where is actor at this instant
            /// </summary>
            public Vector Loc { get; set; }
            /// <summary>
            /// angle in degrees (clock model) to target
            /// </summary>
            public double Angle { get; set; }
            /// <summary>
            /// This is image representing actor on canvas
            /// </summary>
            public Image MyArrow { get; set; }
            /// <summary>
            /// this is Id of this actor
            /// </summary>
            public TextBlock MyId { get; set; }
        }
        /// <summary>
        /// List of actors participating in given simulation
        /// </summary>
        public List<Actor> Actors = new List<Actor>();
        /// <summary>
        /// how many actor are to participate. IS CONSIDERED ONLY ON GO
        /// </summary>
        private int _ActorCount;
        public int ActorCount
        {
            get { return _ActorCount; }
            set {
                _ActorCount = value;
                TextBlockActorCount.Text = _ActorCount.ToString();
            } 
        }
        /// <summary>
        /// Delta time in miliseconds
        /// default value is 1000/30 -> 30 fps in 1 second
        /// </summary>
        private readonly double DTime = (1000/59);
        /// <summary>
        /// this is used to define size of arrows
        /// </summary>
        private readonly double SideSize = 50;
        /// <summary>
        /// default width of window. IS USED iN SIM
        /// </summary>
        private readonly double Width = 1800;
        /// <summary>
        /// default height of window. IS USED iN SIM
        /// </summary>
        private readonly double Height = 900;
        /// <summary>
        /// max speed of actors on screen in pixels
        /// </summary>
        private readonly double MaxSpeed = 5;
        private readonly double Spread = 1.3;
        /// <summary>
        /// clock used for animation. It this thread safe?
        /// </summary>
        private DispatcherTimer clock;
        public MainWindow()
        {
            InitializeComponent();
            clock = new DispatcherTimer();
            clock.Interval = TimeSpan.FromMilliseconds(DTime);
            clock.Tick += update;

            ActorCount = 14;
            Button_GO(null, null);
        }
        /// <summary>
        /// simuation function. is called to update actors and is actual simulation <=================================================================================================================================================
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void update(object? sender, EventArgs e) 
        {//<=================================================================================================================================================
            Actors.ForEach(a =>
            {
                //this vectors
                var Dvector = new Vector();
                //===================================================================repulsors - each actor is reppelled by the others (for asthetics)
                //delta of vectors between actors AND a. this vetors point FROM ac
                var deltaVectors = from actor in Actors
                                   where a.Id != actor.Id// && a.Target.Id != actor.Id
                                   select a.Loc - actor.Loc;
                //increase in power untill cutoff
                double getMultRep(double x) {
                    var result = -(x * x) * .00001 + (SideSize * SideSize * 4) * DTime / 1000;
                    return (result < 0) ? (0) : (result);
                }
                //prep vars
                var moveVectors = from vector in deltaVectors select new {
                    vector = vector,
                    length = vector.Length,
                    mult = getMultRep(vector.Length)
                };
                //add repulsor vectors
                foreach (var move in moveVectors)
                    if (SideSize * Spread > move.length) {
                        var vec = move.vector;
                        vec.Normalize();
                        Dvector = Dvector + (vec * move.mult);
                    }
                Dvector *= .001;
                //Dvector = new Vector(0, 0);
                //===================================================================atractors - actros are attracted to their target and middle of window
                var targetVector = a.Target.Loc - a.Loc;
                var targetLength = targetVector.Length;
                targetVector.Normalize();
                var middleVector = (new Vector(Width/2, Height/2) - a.Loc);
                var middleLength = middleVector.Length;
                middleVector.Normalize();
                //get attractor strength
                double getMultAtr(double x)
                {
                    return x * x * 1 * DTime/1000;
                }
                if (SideSize * Spread < targetLength)
                    Dvector = Dvector + targetVector * getMultAtr(targetLength - (SideSize * Spread));
                if (Height * .5 < middleLength)
                    Dvector = Dvector + middleVector * getMultAtr(middleLength - (Height * .5));
                //===================================================================processing - move and rotate images and stuff
                if (Dvector.Length > MaxSpeed)
                    Dvector = (Dvector / Dvector.Length) * MaxSpeed;

                a.Angle = Vector.AngleBetween(new Vector(0, -1), targetVector);

                a.Loc = a.Loc + Dvector;

                a.MyArrow.RenderTransform = new RotateTransform(a.Angle, SideSize / 2, SideSize / 2);

                Canvas.SetLeft(a.MyArrow, a.Loc.X);
                Canvas.SetTop(a.MyArrow, a.Loc.Y);

                Canvas.SetLeft(a.MyId, a.Loc.X + SideSize * .4);
                Canvas.SetTop(a.MyId, a.Loc.Y + SideSize * .4);
            });
        }//<=================================================================================================================================================
        /// <summary>
        /// Clears and fills 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_GO(object sender, RoutedEventArgs e)
        {
            if (clock.IsEnabled) {
                //STOP!
                clock.Stop();
                //remove all img from canvas
                Actors.ForEach(a => {
                    MyCanvas.Children.Remove(a.MyArrow);
                    MyCanvas.Children.Remove(a.MyId);
                });
                //clear for new sim
                Actors.Clear();
            } else {
                var rand = new Random();
                //create empty actors
                for (int i = 0; i < ActorCount; i++)
                    Actors.Add(new Actor()
                    {
                        Id = i,
                        Loc = new Vector(rand.NextDouble() * Width, rand.NextDouble() * Height),
                        Angle = rand.NextDouble()*360,
                        MyArrow = new Image()
                        {
                            Source = new BitmapImage(new Uri(@"/Icons/Arrow.png", UriKind.Relative)),
                            Height = SideSize,
                            Width = SideSize
                        },
                        MyId = new TextBlock() { Text=i.ToString() }
                    });
                //randomize their targets and add to canvas
                Info.Text = "";
                Actors.ForEach((a) => {
                    while (a.Target == null || a.Target == a)
                        a.Target = Actors[rand.Next(0,ActorCount)];
                    
                    MyCanvas.Children.Add(a.MyArrow);
                    MyCanvas.Children.Add(a.MyId);

                    a.MyArrow.RenderTransform = new RotateTransform(a.Angle, SideSize/2, SideSize/2);

                    Canvas.SetLeft(a.MyArrow, a.Loc.X);
                    Canvas.SetTop(a.MyArrow,a.Loc.Y);

                    Canvas.SetLeft(a.MyId, a.Loc.X+SideSize*.4);
                    Canvas.SetTop(a.MyId, a.Loc.Y+SideSize*.4);

                    Info.Text += "A:" + a.Id + " T:" + a.Target.Id+"|";
                });
                //GO!
                clock.Start();
            }
        }
        /// <summary>
        /// increase ActorCount
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_ADD(object sender, RoutedEventArgs e)
        {
            ActorCount++;
        }
        /// <summary>
        /// Decrease ActorCount
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_SUB(object sender, RoutedEventArgs e)
        {
            ActorCount--;
        }
    }
}
