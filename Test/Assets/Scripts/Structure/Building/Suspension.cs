using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const;

namespace Architecture
{
    public struct Suspension
    {
        public ISuspension center;
        public ISuspension left;
        public ISuspension down;
        public ISuspension right;

        public bool isPermanent
        {
            get
            {
                return (center != null ||
                    left is Tile ||
                    right is Tile ||
                    down is Tile
                    );
            }
        }

        public int Count
        {
            get
            {
                return (center == null ? 0 : 1) +
                (left == null ? 0 : 1) +
                (right == null ? 0 : 1) +
                (down == null ? 0 : 1);
            }
        }

        public bool HasSuspension(GridDirection direction)
        {
            switch (direction)
            {
                case GridDirection.LEFT:
                    return left != null;

                case GridDirection.RIGHT:
                    return right != null;

                case GridDirection.DOWN:
                    return down != null;
            }

            return false;
        }

        public void DestroySuspension(GridDirection direction)
        {
            switch (direction)
            {
                case GridDirection.LEFT:
                    left = null;
                    break;

                case GridDirection.RIGHT:
                    right = null;
                    break;

                case GridDirection.DOWN:
                    down = null;
                    break;
            }
        }

        public void Add(GridDirection direction, Building building)
        {
            switch (direction)
            {
                case GridDirection.LEFT:
                    left = left == null ? building : left;
                    break;

                case GridDirection.RIGHT:
                    right = right == null ? building : right;
                    break;

                case GridDirection.DOWN:
                    down = down == null ? building : down;
                    break;
            }
        }

        public void DoForAll(Action<GridDirection, ISuspension> action)
        {
            if (left != null)
                action(GridDirection.RIGHT, left);

            if (down != null)
                action(GridDirection.UP, down);

            if (right != null)
                action(GridDirection.LEFT, right);
        }

        public override string ToString()
        {
            return string.Format("Center: {0} Left: {1}, Right: {2}, Down: {3}", center != null, left != null, right != null, down != null);
        }
    }
}
