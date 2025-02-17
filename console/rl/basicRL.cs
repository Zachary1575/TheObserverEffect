using System;

/*
* This class should create an simple AI object where when invoked, should produce a output coordinate
* such that the AI may be able to travel in a 2D environment.
*/

namespace basicRL {
    public class QLearning {

        private bool moveRandomUniform;
        private Random random;

        public QLearning() {
            moveRandomUniform = true; // Just for testing purposes
            random = new Random();
        }

        public int[] getOptimalCoordinate(int playerX, int playerY) {
            int[] optimalCoordinate = [0, 0];
    
            if (moveRandomUniform) {
                optimalCoordinate[0] = random.Next(0, 19 + 1);
                optimalCoordinate[1] = random.Next(0, 19 + 1);
            }

            // Need to get optimalCoordinates using the Bellman Equation...
            // Calculate states 2 blocks away. It can move two blocks or one.
            
            return optimalCoordinate;
        }


    }
}