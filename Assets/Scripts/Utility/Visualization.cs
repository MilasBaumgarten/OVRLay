using UnityEngine;

namespace Utility {
    public static class Visualization {
        public static void ShiftLineRendererPositions(LineRenderer lineRenderer, Vector3 newPosition) {
            // store old positions into an array
            var positions = new Vector3[lineRenderer.positionCount];
            for (var i = positions.Length - 1; i > 0; i--) {
                positions[i-1] = lineRenderer.GetPosition(i);
            }

            // add the new position to the end
            positions[positions.Length - 1] = newPosition;
            // apply the positions
            lineRenderer.SetPositions(positions);
        }
    }
}
