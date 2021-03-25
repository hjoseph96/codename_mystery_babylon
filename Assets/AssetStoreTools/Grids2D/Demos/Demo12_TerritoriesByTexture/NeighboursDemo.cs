using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Grids2D {
    public class NeighboursDemo : MonoBehaviour {

        Grid2D grid;
        void Start() {
            grid = Grid2D.instance;
            grid.OnTerritoryClick += OnTerritoryClick;

        }

        private void OnTerritoryClick(int territoryIndex) {
            // Blink neighbours
            List<Territory> neighbours = grid.TerritoryGetNeighbours(territoryIndex);
            neighbours.ForEach((Territory t) => {
                int neighbourIndex = grid.TerritoryGetIndex(t);
                grid.TerritoryBlink(neighbourIndex, Color.blue, 1f);
            });
        }

    }

}