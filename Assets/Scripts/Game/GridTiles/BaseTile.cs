using UnityEngine;

namespace Com.Hypester.DM3
{
    public class BaseTile : MonoBehaviour
    {
        public Vector2 position { get; set; }

        private void Update()
        {
            Vector2 goalPosition = new Vector2();
            if (position.x % 2 == 0)
                goalPosition = new Vector2((-Constants.gridXsize / 2 + position.x) * Constants.tileWidth + Constants.tileWidth / 2, (-Constants.gridYsize / 2 + position.y) * Constants.tileHeight + (Constants.tileHeight * .75f));
            else
                goalPosition = new Vector2((-Constants.gridXsize / 2 + position.x) * Constants.tileWidth + Constants.tileWidth / 2, (-Constants.gridYsize / 2 + position.y) * Constants.tileHeight + (Constants.tileHeight * 0.25f));

            Vector2.MoveTowards(transform.localPosition, goalPosition, .1f);
        }

        public void ShiftTileDown ()
        {
            float newY = Mathf.Max(position.y - 1f, 0f);
            position = new Vector2 (position.x, newY);
            gameObject.name = "Tile (" + position.x + "," + newY + ")";
        }

        public void ShiftTileUp()
        {
            float newY = Mathf.Min(position.y + 1f, Constants.gridYsize - 1);
            position = new Vector2(position.x, newY);
            gameObject.name = "Tile (" + position.x + "," + newY + ")";
        }
    }
}