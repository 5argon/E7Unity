// Sirawat Pitaksarit / 5argon - Exceed7 Experiments
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(GridLayoutGroup))]
/// <summary>
/// Instead of constraining only row or column, constrain both and let the cell size fill the parent.
/// </summary>
public class DynamicGridLayoutGroup : MonoBehaviour
{
    public int column = 1;
	public int row = 1;

	private int Column {
		get{
			if( column < 1)
			{
				column = 1;
			}
			return column;
		}
	}

	private int Row {
		get{
			if( row < 1)
			{
				row = 1;
			}
			return row;
		}
	}

	private RectTransform parent;
	private GridLayoutGroup grid;

	void Awake()
	{
        parent = gameObject.GetComponent<RectTransform>();
        grid = gameObject.GetComponent<GridLayoutGroup>();
	}

    void Update()
    {
		if(parent == null)
		{
			parent = gameObject.GetComponent<RectTransform>();
		}
		if(grid == null)
		{
			grid = gameObject.GetComponent<GridLayoutGroup>();
		}

		grid.constraint = GridLayoutGroup.Constraint.Flexible;

        float cellX = (parent.rect.width / Column) - ((grid.spacing.x * (Column - 1))/Column) - (grid.padding.left/(float)Column) - (grid.padding.right/(float)Column);
        float cellY = (parent.rect.height / row) - ((grid.spacing.y * (Row - 1))/Row) - (grid.padding.top/(float)Row) - (grid.padding.bottom/(float)Row);
	   if(float.IsInfinity(cellX) || float.IsNaN(cellX) || float.IsInfinity(cellY) || float.IsNaN(cellY))
	   {
		   grid.cellSize = Vector2.zero;
	   }
	   else
	   {
		   grid.cellSize = new Vector2(cellX,cellY);
	   }
    }
}