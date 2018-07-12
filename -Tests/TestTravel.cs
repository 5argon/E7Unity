using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NUnit.Framework;

namespace E7Unity.Tests
{
    public class TestTravel
    {
        [Test]
        public void Instantiating()
        {
            Travel<int> t1 = new Travel<int>();
            Travel<float> t2 = new Travel<float>();
            t1.Init();
            t2.Init();
            t1.Dispose();
            t2.Dispose();
        }

        private Travel<int> MakeTravel 
        {
            get{
                Travel<int> t = new Travel<int>();
                t.Init();
                t.Add(0,0,500);
                t.Add(10,5,600);
                t.Add(20,5,700);
                t.Add(40,5,800);
                return t;
            }
        }

        [Test]
        public void Adding()
        {
            Travel<int> t = MakeTravel;
            t.Dispose();
        }

        [Test]
        public void FirstAndLast()
        {
            Travel<int> t = MakeTravel;
            Assert.That(t.FirstEvent.Time, Is.Zero);
            Assert.That(t.FirstEvent.Position, Is.Zero);
            Assert.That(t.FirstData , Is.EqualTo(500));

            Assert.That(t.LastEvent.Time, Is.EqualTo(15));
            Assert.That(t.LastEvent.Position, Is.EqualTo(40));
            Assert.That(t.LastData, Is.EqualTo(800));
            t.Dispose();
        }

        [Test]
        public void DataOfTime()
        {
            Travel<int> t = MakeTravel;
            Assert.That(t.DataEventOfTime(6).data, Is.EqualTo(600));
            Assert.That(t.DataEventOfTime(555).data, Is.EqualTo(800));

            Assert.That(t.DataEventOfTime(6).travelEvent.Position, Is.EqualTo(10));
            Assert.That(t.DataEventOfTime(555).travelEvent.Position, Is.EqualTo(40));

            Assert.That(t.DataEventOfPosition(30).data, Is.EqualTo(700));
            Assert.That(t.DataEventOfPosition(555).data, Is.EqualTo(800));

            t.Dispose();
        }

        [Test]
        public void AddDefaultAtZero()
        {
            Travel<int> t = new Travel<int>();
            t.Init();
            t.Add(10, 5, 600);
            t.Add(20, 5, 700);
            t.Add(40, 5, 800);
            Assert.That(t.DataEventOfTime(2).data, Is.EqualTo(default(int)));
            t.Dispose();

            t = new Travel<int>();
            t.Init();
            t.AddDefaultAtZero(555);
            Assert.That(t.DataEventOfTime(2).data, Is.EqualTo(555));
            t.Add(10, 5, 600);
            t.Add(20, 5, 700);
            t.Add(40, 5, 800);
            t.AddDefaultAtZero(5555);
            Assert.That(t.DataEventOfTime(2).data, Is.EqualTo(555), "Still the same since there is already something at zero.");

            t.Dispose();
        }

        [Test]
        public void NextPrevious()
        {
            Travel<int> t = MakeTravel;
            TravelEvent te = t.DataEventOfPosition(30).travelEvent;
            Assert.That(t.NextOf(te).travelEvent.Position,Is.EqualTo(40) );
            Assert.That(t.PreviousOf(te).travelEvent.Position, Is.EqualTo(10));
            Assert.That(t.NextOf(te).data,Is.EqualTo(800) );

            t.Dispose();
        }

    }
}