using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const;

namespace Architecture
{
    public struct Neighbors
    {
        public Building left;
        public Building up;
        public Building right;

        public int Count
        {
            get
            {
                return (left == null ? 0 : 1) +
                (right == null ? 0 : 1) +
                (up == null ? 0 : 1);
            }
        }

        public void Destroy(GridDirection direction)
        {
            switch (direction)
            {
                case GridDirection.LEFT:
                    left = null;
                    break;

                case GridDirection.RIGHT:
                    right = null;
                    break;

                case GridDirection.UP:
                    up = null;
                    break;
            }
        }

        public void Add(GridDirection direction, Building building)
        {
            switch (direction)
            {
                case GridDirection.LEFT:
                    left = building;
                    break;

                case GridDirection.RIGHT:
                    right = building;
                    break;

                case GridDirection.UP:
                    up = building;
                    break;
            }
        }

        public void DoForAll(Action<GridDirection, Building> action)
        {
            if (left != null)
                action(GridDirection.RIGHT, left);

            if (up != null)
                action(GridDirection.DOWN, up);

            if (right != null)
                action(GridDirection.LEFT, right);
        }

        public void DoForSide(Action<GridDirection, Building> action)
        {
            if (left != null)
                action(GridDirection.RIGHT, left);

            if (up != null)
                action(GridDirection.DOWN, up);

            if (right != null)
                action(GridDirection.LEFT, right);
        }

        public override string ToString()
        {
            return string.Format("Left: {0}, Right: {1}, Up: {2}", left != null, right != null, up != null);
        }
    }
}