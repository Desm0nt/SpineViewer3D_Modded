using System.Collections.Generic;
using System.Drawing;
using SpineLib.Geometry.Descriptions;
using Dicom;
using System;

namespace SpineLib.Geometry
{
    public class SpineStorage
    {
        private Dictionary<string, SpineDescription> descriptions;
        private Dictionary<string, SpinousProcessDescription> spinousprocessdecriptions;
        
        private List<Point> markerPoints = new List<Point>();

        public short windowWidth = 0;
        public short windowCenter = 0;

        private int rotatingAngle = 0;

        public byte direction = 0;
        //0 - Left
        //1 - Right
        //2 - Forward
        //3 - Backward

        public int imageDirection = 0;
        //0 - Front
        //1 - Side

        private Tuple<Tuple<int, int, int>, Tuple<int, int, int>> markerLine = null;

        private double markerLength = 1.0;
        private double markerSize = 1.0;

        public SpineStorage() {
            descriptions = new Dictionary<string, SpineDescription>();
            spinousprocessdecriptions = new Dictionary<string, SpinousProcessDescription>();
        }

        public void AddDescription(string key, SpineDescription state) {
            if (state != null) {
                descriptions[key] = state;
            }
        }

        public void RemoveDescription(string key)
        {
            if (descriptions.ContainsKey(key))
            {
                descriptions.Remove(key);
            }
        }

        public SpineDescription GetDescription(string key)
        {
            if (descriptions.ContainsKey(key))
            {
                return descriptions[key];
            }
            else {
                return null;
            }
        }

        public bool ContainDescription(string key) {
            return descriptions.ContainsKey(key);
        }

        public void AddSpinousProcessDescription(string key, SpinousProcessDescription state)
        {
            if (state != null)
            {
                state.Direction = this.direction;
                spinousprocessdecriptions[key] = state;
            }
        }

        public void RemoveSpinousProcessDescription(string key)
        {
            if (spinousprocessdecriptions.ContainsKey(key))
            {
                spinousprocessdecriptions.Remove(key);
            }
        }

        public SpinousProcessDescription GetSpinousProcessDescription(string key)
        {
            if (spinousprocessdecriptions.ContainsKey(key))
            {
                return spinousprocessdecriptions[key];
            }
            else
            {
                return null;
            }
        }

        public bool ContainSpinousProcessDescription(string key)
        {
            return spinousprocessdecriptions.ContainsKey(key);
        }

        public List<string> Keys {
            get {
                var list = new List<string>();
                foreach (var key in descriptions.Keys)
                {
                    list.Add(key);
                }
                return list;
            }
        }

        public List<string> SpinousProcessKeys
        {
            get
            {
                var list = new List<string>();
                foreach (var key in spinousprocessdecriptions.Keys)
                {
                    list.Add(key);
                }
                return list;
            }
        }

        public Tuple<Tuple<int, int, int>, Tuple<int, int, int>> MarkerLine
        {
            get
            {
                return markerLine;
            }

            set
            {
                markerLine = value;
            }
        }

        public double MarkerSize
        {
            get
            {
                return markerSize;
            }

            set
            {
                markerSize = value;
            }
        }

        public double MarkerLength
        {
            get
            {
                return markerLength;
            }

            set
            {
                markerLength = value;
            }
        }

        public int GetRotatingAngle()
        {
            return rotatingAngle;
        }

        public void SetRotatingAngle(int angle) {
            rotatingAngle = (angle + 360) % 360;
        }

        public Point GetMarkerPoint(int i) {
            return markerPoints[i];
        }

        public void SetMarkerPoint(int i, Point point)
        {
            if (i >= markerPoints.Count)
            {
                markerPoints.Add(point);
            }
            else {
                markerPoints[i] = point;
            }
        }

        public int GetMarkersCount() {
            return markerPoints.Count;
        }

        public void RecalcDirections(int imageWidth, int imageHeight) {
      
            if (markerPoints.Count == 4) {

                var origin = new Point(imageWidth / 2, imageHeight / 2);
                var mpoints = GeometryHelper.RotatePoints(markerPoints, origin, -1 * rotatingAngle);

                var headPoint = mpoints[0];
                var footPoint = mpoints[1];

                var heart_abd_point = mpoints[2];
                var noheart_back_point = mpoints[3];
                

                if (imageDirection == 0) // front
                {
                    if (headPoint.Y < footPoint.Y)
                    {
                        if (heart_abd_point.X > noheart_back_point.X)
                        {
                            direction = 3;
                        }
                        else
                        {
                            direction = 2;
                        }
                    }
                    else
                    {
                        if (heart_abd_point.X > noheart_back_point.X)
                        {
                            direction = 2;
                        }
                        else
                        {
                            direction = 3;
                        }
                    }

                }
                else if (imageDirection == 1) // side
                {
                    if (headPoint.Y < footPoint.Y)
                    {
                        if (heart_abd_point.X > noheart_back_point.X)
                        {
                            direction = 1;
                        }
                        else
                        {
                            direction = 0;
                        }
                    }
                    else {
                        if (heart_abd_point.X > noheart_back_point.X)
                        {
                            direction = 0;
                        }
                        else
                        {
                            direction = 1;
                        }
                    }

                }
            }
        }



        public static SpineStorage GenerateStorageForAddedFile(DicomFile file) {
            var storage = new SpineStorage();

            var window_params = DicomUtils.GetWindowParameters(file);
            storage.windowWidth = window_params.Item1;
            storage.windowCenter = window_params.Item2;
            
            storage.SetRotatingAngle(0);

            storage.direction = 0;
            storage.imageDirection = 0;

            return storage;
        }

    }
}
