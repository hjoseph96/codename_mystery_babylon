using UnityEngine;
using System.Collections;

namespace SS.TwoD
{
    public class DirectionTools
    {
        int[] m_DirectionArray;
        int m_MaxDirection;

        public DirectionTools(int maxDirection)
        {
            m_MaxDirection = maxDirection;
            m_DirectionArray = new int[maxDirection];

            int startIndex = maxDirection / 4 - 1;
            int currentIndex = 0;

            for (int i = startIndex; i >= 0; i--)
            {
                m_DirectionArray[currentIndex] = i;
                currentIndex++;
            }

            for (int i = maxDirection - 1; i > startIndex; i--)
            {
                m_DirectionArray[currentIndex] = i;
                currentIndex++;
            }
        }

        public int GetDirection(Vector3 source, Vector3 target)
        {
            float x = target.x - source.x;
            float z = target.z - source.z;

            return GetDirection(x, z);
        }

        public int GetDirection(float x, float z)
        {
            float alpha = Mathf.Atan2(z, x) * Mathf.Rad2Deg;

            return GetDirection(alpha);
        }

        public int GetDirection(float alpha)
        {
            if (alpha < 0)
            {
                alpha += 360;
            }

            float range = 360f / m_MaxDirection;
            float startAngle = range / 2;
            float currentAngle = startAngle;

            for (int i = 0; i < m_MaxDirection - 1; i++)
            {
                float angle = range;
                if (alpha >= currentAngle && alpha < currentAngle + angle)
                {
                    return m_DirectionArray[i];
                }
                currentAngle += angle;
            }

            if ((alpha >= 360 - startAngle && alpha < 360f) || (alpha >= 0 && alpha < startAngle))
            {
                return m_DirectionArray[m_MaxDirection - 1];
            }

            return 0;
        }
    }
}
