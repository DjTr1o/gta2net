﻿//Created: 16.02.2010

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hiale.GTA2NET.Core.Map;
using Microsoft.Xna.Framework;
using Hiale.GTA2NET.Helper;
using Hiale.GTA2.Core.Map;

namespace Hiale.GTA2NET.Logic
{
    public class MovableObject
    {
        public static event EventHandler<GenericEventArgs<MovableObject>> ObjectCreated;
        public event EventHandler PositionChanged;
        public event EventHandler RotationChanged;

        private Vector3 _position3;
        /// <summary>
        /// Current position of this object. It represents the centre of the object.
        /// </summary>
        public Vector3 Position3
        {
            get { return _position3; }
            set
            {
                _position3 = value;
                //if (PositionChanged != null)
                //    PositionChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 2D position of the object.
        /// </summary>
        public Vector2 Position2
        {
            get
            {
                return new Vector2(Position3.X, Position3.Y);
            }
        }


        private Vector2 _origin;
        /// <summary>
        /// Origin of the object in 2D space.
        /// </summary>
        public Vector2 Origin
        {
            get
            {
                if (_origin == Vector2.Zero)
                    _origin = new Vector2(Width / 2, Height / 2);
                return _origin;
            }
            //set { _origin = value; }
        }

        private const float Circumference = 2 * (float)Math.PI;

        private float _rotationAngle;
        /// <summary>
        /// Current rotation angle in radians.
        /// </summary>
        public float RotationAngle
        {
            get { return _rotationAngle; }
            set
            {
                if (value < -Circumference)
                    value += Circumference;
                else if (value > Circumference)
                    value -= Circumference;
                _rotationAngle = value;
            }
        }

        /// <summary>
        /// 2D top left point of the object.
        /// </summary>
        public Vector2 TopLeft2
        {
            get
            {
                Vector2 topLeft = new Vector2(Position3.X - (Width / 2), Position3.Y - (Height / 2));
                return RotatePoint(topLeft, Position2, RotationAngle);
            }
        }

        private float _topLeftZ;

        private bool _topLeftZUpdated;

        /// <summary>
        /// 3D top left point of the object.
        /// </summary>
        public Vector3 TopLeft3
        {
            get
            {
                Vector2 topLeft = TopLeft2;
                return new Vector3(topLeft.X, topLeft.Y, _topLeftZ);
            }
        }
        

        /// <summary>
        /// 2D top right point of the object.
        /// </summary>
        public Vector2 TopRight2
        {
            get
            {
                Vector2 topRight = new Vector2(Position3.X + (Width / 2), Position3.Y - (Height / 2));
                return RotatePoint(topRight, Position2, RotationAngle);
            }
        }

        private float _topRightZ;

        private bool _topRightZUpdated;

        /// <summary>
        /// 3D top right point of the object.
        /// </summary>
        public Vector3 TopRight3
        {
            get
            {
                Vector2 topRight = TopRight2;
                return new Vector3(topRight.X, topRight.Y, _topRightZ);
            }
        }

        /// <summary>
        /// 2D bottom right point of the object.
        /// </summary>
        public Vector2 BottomRight2
        {
            get
            {
                Vector2 bottomRight = new Vector2(Position3.X + (Width / 2), Position3.Y + (Height / 2));
                return RotatePoint(bottomRight, Position2, RotationAngle);
            }
        }

        private float _bottomRightZ;

        private bool _bottomRightZUpdated;

        /// <summary>
        /// 3D bottom right point of the object.
        /// </summary>
        public Vector3 BottomRight3
        {
            get
            {
                Vector2 bottomRight = BottomRight2;
                return new Vector3(bottomRight.X, bottomRight.Y, _bottomRightZ);
            }
        }

        /// <summary>
        /// 2D bottom left point of the object.
        /// </summary>
        public Vector2 BottomLeft2
        {
            get
            {
                Vector2 bottomLeft = new Vector2(Position3.X - (Width / 2), Position3.Y + (Height / 2));
                return RotatePoint(bottomLeft, Position2, RotationAngle);
            }
        }

        private float _bottomLeftZ;

        private bool _bottomLeftZUpdated;

        /// <summary>
        /// 3D bottom left point of the object.
        /// </summary>
        public Vector3 BottomLeft3
        {
            get
            {
                Vector2 bottomLeft = BottomLeft2;
                return new Vector3(bottomLeft.X, bottomLeft.Y, _bottomLeftZ);
            }
        }

        private float _width;
        /// <summary>
        /// Width of the object.
        /// </summary>
        public float Width
        {
            get { return _width; }
        }

        private float _height;
        /// <summary>
        /// Height of the object.
        /// </summary>
        public float Height
        {
            get { return _height; }
        }

        /// <summary>
        /// Helper variable to calculate the distance moved.
        /// </summary>
        private static readonly Vector2 OriginZero = Vector2.Zero;

        protected float Velocity;

        public MovableObject(Vector3 position) //ToDo: Add SpriteNumber(s), Width, Height, StartUpRotation
        {
            _position3 = position;

            //NEW 04.03.2010
            _topLeftZ = position.Z;
            _topRightZ = position.Z;
            _bottomRightZ = position.Z;
            _bottomLeftZ = position.Z;

            if (ObjectCreated != null)
                ObjectCreated(this, new GenericEventArgs<MovableObject>(this));
        }

        /// <summary>
        /// Temporary helper method to set the width and height. Will be moved to the constructor in the future.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void SetDimension(float width, float height) //ToDo: move to constructor
        {
            _width = width;
            _height = height;
        }


        /// <summary>
        /// Moves the object forward or backwards and changes the rotation angle.
        /// </summary>
        /// <param name="forwardChange">Positive values mean 'go forward', negative 'go backward'</param>
        /// <param name="rotationChange">ToDo</param>
        public void Move(float forwardChange, float rotationChange)
        {
            if (forwardChange < 0) //Backwards
                rotationChange *= -1;

            if (forwardChange == 0)
                return;

            float rotationAngleNew = RotationAngle;
            rotationAngleNew += MathHelper.ToRadians(rotationChange);
            Vector2 direction = RotatePoint(new Vector2(0, forwardChange), OriginZero, rotationAngleNew);

            CheckCollision(ref direction);
            RotationAngle = rotationAngleNew;

            //direction.X = (float) Math.Round(direction.X, 4);
            //direction.Y = (float)Math.Round(direction.Y, 4);

            float axis1 = MathHelper.Lerp(_topLeftZ, _bottomRightZ, 0.5f);
            float axis2 = MathHelper.Lerp(_topRightZ, _bottomLeftZ, 0.5f);
            float weightedHeight = MathHelper.Lerp(axis1, axis2, 0.5f);

            Position3 = new Vector3(Position3.X + direction.X, Position3.Y + direction.Y, weightedHeight);

            if (PositionChanged != null)
                PositionChanged(this, EventArgs.Empty);
        }


        private void CheckCollision(ref Vector2 direction)
        {
            Vector2 topLeft = TopLeft2; //Create these for cache
            Vector2 topRight = TopRight2;
            Vector2 bottomRight = BottomRight2;
            Vector2 bottomLeft = BottomLeft2;

            Vector2 newTopLeft = topLeft + direction;
            Vector2 newTopRight = topRight + direction;
            Vector2 newBottomRight = bottomRight + direction;
            Vector2 newBottomLeft = bottomLeft + direction;

            int minBlockX = (int)newTopLeft.X;
            int maxBlockX = (int)newTopLeft.X;
            int minBlockY = (int)newTopLeft.Y;
            int maxBlockY = (int)newTopLeft.Y;
            SetMinMax(ref minBlockX, ref maxBlockX, newTopLeft.X);
            SetMinMax(ref minBlockY, ref maxBlockY, newTopLeft.Y);
            SetMinMax(ref minBlockX, ref maxBlockX, newTopRight.X);
            SetMinMax(ref minBlockY, ref maxBlockY, newTopRight.Y);
            SetMinMax(ref minBlockX, ref maxBlockX, newBottomRight.X);
            SetMinMax(ref minBlockY, ref maxBlockY, newBottomRight.Y);
            SetMinMax(ref minBlockX, ref maxBlockX, newBottomLeft.X);
            SetMinMax(ref minBlockY, ref maxBlockY, newBottomLeft.Y);

            int minBlockZ = (int)Position3.Z;
            int maxBlockZ = (int)Position3.Z;
            float newZ;
            if (_topLeftZUpdated)
            {
                newZ = _topLeftZ;
                SetCorrectHeight(ref newZ, newTopLeft);
                SetMinMax(ref minBlockZ, ref maxBlockZ, newZ);
            }
            if (_topRightZUpdated)
            {
                newZ = _topRightZ;
                SetCorrectHeight(ref newZ, newTopRight);
                SetMinMax(ref minBlockZ, ref maxBlockZ, newZ);
            }
            if (_bottomRightZUpdated)
            {
                newZ = _bottomRightZ;
                SetCorrectHeight(ref newZ, newBottomRight);
                SetMinMax(ref minBlockZ, ref maxBlockZ, newZ);
            }
            if (_bottomLeftZUpdated)
            {
                newZ = _bottomLeftZ;
                SetCorrectHeight(ref newZ, newBottomLeft);
                SetMinMax(ref minBlockZ, ref maxBlockZ, newZ);
            }
            minBlockZ = minBlockZ - 1;
            maxBlockZ = maxBlockZ + 1;
            if (minBlockZ < 0)
                minBlockZ = 0;
            if (maxBlockZ > 7)
                maxBlockZ = 7;

            for (int x = minBlockX; x < maxBlockX + 1; x++)
            {
                for (int y = minBlockY; y < maxBlockY + 1; y++)
                {
                    for (int z = minBlockZ; z < maxBlockZ + 1; z++)
                    {
                        CheckBlock(ref x, ref y, ref z, ref newTopLeft, ref newTopRight, ref newBottomRight, ref newBottomLeft, ref direction);
                    }
                }
            }
            _topLeftZUpdated = SetCorrectHeight(ref _topLeftZ, topLeft + direction);
            _topRightZUpdated = SetCorrectHeight(ref _topRightZ, topRight + direction);
            _bottomRightZUpdated = SetCorrectHeight(ref _bottomRightZ, bottomRight + direction);
            _bottomLeftZUpdated = SetCorrectHeight(ref _bottomLeftZ, bottomLeft + direction);
        }

        private void CheckBlock(ref int x, ref int y, ref int z, ref Vector2 newTopLeft, ref Vector2 newTopRight, ref Vector2 newBottomRight, ref Vector2 newBottomLeft, ref Vector2 direction)
        {
            BlockInfo block = MainGame.Map.CityBlocks[x, y, z];
            //if (x == 79 && y == 181 && z >= 3)
            //    System.Diagnostics.Debug.WriteLine("OK");

            //if (x == 66 && y == 187)
            //    System.Diagnostics.Debug.WriteLine("OK");

            //if (x == 58 && y == 195)
            //    System.Diagnostics.Debug.WriteLine("OK");

            bool movableSlope = false; //a movable Slope is a block which actually intersecs with the object, but the object can move above it to change the height.
            bool blockAboveStops = false;
            if (!ProcessBlock(ref block, ref x, ref y, ref z, ref movableSlope, ref blockAboveStops))
                return;
            //if (!block.LidOnly || movableSlope || blockAboveStops)
            if (movableSlope || blockAboveStops)
            {
                Polygon polygonObject = new Polygon();
                polygonObject.Points.Add(newTopLeft);
                polygonObject.Points.Add(newTopRight);
                polygonObject.Points.Add(newBottomRight);
                polygonObject.Points.Add(newBottomLeft);

                BlockInfo blockPolygon = block;
                if (blockAboveStops)
                    blockPolygon = MainGame.Map.CityBlocks[x, y, z + 1];
                Polygon polygonBlock = CreateBlockPolygon(ref blockPolygon, ref x, ref y);
                PolygonCollisionResult resNew = SeparatingAxisTheorem.PolygonCollision(ref polygonObject, ref polygonBlock, ref direction);
                if (resNew.Intersect)
                {
                    if (block.IsLowSlope || block.IsHighSlope)
                    {
                        return;
                    }

                    //direction.X = 0;
                    //direction.Y = 0;
                    direction.X = resNew.MinimumTranslationVector.X;
                    direction.Y = resNew.MinimumTranslationVector.Y;
                    return;
                }
            }
        }

        private bool ProcessBlock(ref BlockInfo block, ref int x, ref int y, ref int z, ref bool movableSlope, ref bool blockAboveStops)
        {
            if (Position3.Z % 1 != 0) //07.03.2010, let's see if it works...
                return false;

            movableSlope = false;

            int currentZ = (int) Position3.Z;
            
            //check the block above the current block. If this block is empty, process the current block.
            BlockInfo blockAbove = MainGame.Map.CityBlocks[x, y, currentZ + 1];
            if (z == currentZ && !blockAbove.IsLowSlope && !blockAbove.IsHighSlope)
            {
                //if (!blockAbove.IsEmpty && !blockAbove.IsDiagonalSlope)
                if (!blockAbove.IsEmpty)
                    blockAboveStops = true;
                return true;
            }

            if (block.IsEmpty) //new 6.3.2010
                return false;

            //check one block above only if it's a diagnoal or a low/high slope
            if (z == currentZ + 1)
            {
                if (block.IsDiagonalSlope)
                    return true;
                if (block.IsLowSlope || block.IsHighSlope)
                {
                    movableSlope = true;
                    return true;
                }
            }

            return false;
        }

        private static void SetMinMax(ref int minBlock, ref int maxBlock, float currentValue)
        {
            if (currentValue < minBlock)
                minBlock = (int)currentValue;
            if (currentValue > maxBlock)
                maxBlock = (int)currentValue;
        }

        private static Polygon CreateBlockPolygon(ref BlockInfo block, ref int x, ref int y)
        {
            Polygon polygon = new Polygon();
            SlopeType slope = block.SlopeType;

            switch (slope)
            {
                case SlopeType.None:
                    polygon.Points.Add(new Vector2(x, y));
                    polygon.Points.Add(new Vector2(x + 1, y));
                    polygon.Points.Add(new Vector2(x + 1, y + 1));
                    polygon.Points.Add(new Vector2(x, y + 1));
                    break;
                case SlopeType.DiagonalFacingDownLeft:
                    polygon.Points.Add(new Vector2(x, y));
                    polygon.Points.Add(new Vector2(x + 1, y));
                    polygon.Points.Add(new Vector2(x + 1, y + 1));
                    break;
                case SlopeType.DiagonalFacingDownRight:
                    polygon.Points.Add(new Vector2(x, y));
                    polygon.Points.Add(new Vector2(x + 1, y));
                    polygon.Points.Add(new Vector2(x, y + 1));
                    break;
                case SlopeType.DiagonalFacingUpLeft:
                    polygon.Points.Add(new Vector2(x + 1, y));
                    polygon.Points.Add(new Vector2(x + 1, y + 1));
                    polygon.Points.Add(new Vector2(x, y + 1));
                    break;
                case SlopeType.DiagonalFacingUpRight:
                    polygon.Points.Add(new Vector2(x, y));
                    polygon.Points.Add(new Vector2(x + 1, y + 1));
                    polygon.Points.Add(new Vector2(x, y + 1));
                    break;
                default: //ToDo: implement all slopes!
                    polygon.Points.Add(new Vector2(x, y));
                    polygon.Points.Add(new Vector2(x + 1, y));
                    polygon.Points.Add(new Vector2(x + 1, y + 1));
                    polygon.Points.Add(new Vector2(x, y + 1));
                    break;
            }
            return polygon;
        }

        private static bool SetCorrectHeight(ref float value, Vector2 point)
        {
            float newValue = MainGame.GetHighestPointF(point.X, point.Y);
            if (newValue != value && Math.Abs(newValue - value) < 1)
            {
                value = newValue;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Rotate a point from a given location and adjust using the Origin we
        /// are rotating around
        /// </summary>
        /// <param name="point">The point to rotate.</param>
        /// <param name="origin">Used origin for the rotation.</param>
        /// <param name="rotation">Angle in radians.</param>
        /// <returns></returns>
        public Vector2 RotatePoint(Vector2 point, Vector2 origin, float rotation)
        {
            Vector2 aTranslatedPoint = new Vector2();
            aTranslatedPoint.X = (float)(origin.X + (point.X - origin.X) * Math.Cos(rotation)
                - (point.Y - origin.Y) * Math.Sin(rotation));
            aTranslatedPoint.Y = (float)(origin.Y + (point.Y - origin.Y) * Math.Cos(rotation)
                + (point.X - origin.X) * Math.Sin(rotation));
            return aTranslatedPoint;
        }
    }
}
