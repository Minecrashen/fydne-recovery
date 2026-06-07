using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Qurre.API.Addons.Models;

public class Mazer
{
    static readonly Color32 _color = new(40, 43, 33, 255);

    readonly List<Cell> Walls = new();
    List<Cell> Cells = new();

    readonly HashSet<ModelPrimitive> _escapes = new();
    ModelPrimitive _mainCube;

    void MazeIt(Model model)
    {
        int max = 10000;
        int size = 100;
        int sizer = (size - 1) / 2;
        float height = 3;
        Debug.Log("size : " + size + " r " + sizer);

        for (int x = -sizer; x <= sizer; x++)
        {
            for (int z = -sizer; z <= sizer; z++)
            {
                Cells.Add(new Cell(x, z));
            }
        }



        Cell startingCell = GetCellAt(0, 0);
        Walls.Add(startingCell);

        while (true)
        {

            Cell wall = Walls[Random.Range(0, Walls.Count)];

            ProcessWall(wall);


            if (Walls.Count <= 0)
                break;
            if (--max < 0)
                break;

        }

        Cells = Cells.Where(x => x.Wall).ToList();
        Cell startedCell = null;
        for (int i = 0; i < Cells.Count; i++)
        {
            Cell cell = Cells[i];
            if (!cell.Wall)
            {
                continue;
            }

            startedCell ??= cell;

            Cell nextCell = null;
            if (Cells.Count - 1 > i)
            {
                nextCell = Cells[i + 1];
            }

            if (nextCell != null && nextCell.X == cell.X && nextCell.Z == cell.Z + 1)
            {
                continue;
            }

            ModelPrimitive cube = new(model, PrimitiveType.Cube, _color, Vector3.zero, Vector3.zero);
            cube.GameObject.transform.localScale = new Vector3(1, height, cell.Z - startedCell.Z + 1);
            cube.GameObject.transform.localPosition = new Vector3(cell.X, 0f, cell.Z - ((cell.Z - startedCell.Z) / 2));
            model.AddPart(cube);

            if (cube.GameObject.transform.localScale.z >= (size - 1) && cube.GameObject.transform.localPosition.z == 0)
            {
                cube.Primitive.Color = Color.yellow;
                _escapes.Add(cube);
            }
            else if ((cell.Z == sizer || startedCell.Z == -sizer) && (cube.GameObject.transform.localScale.z % 2 == 0))
            {
                cube.GameObject.transform.localPosition = new Vector3(cube.GameObject.transform.localPosition.x, 0f, cube.GameObject.transform.localPosition.z - 0.5f);
            }

            startedCell = null;
        }

        {
            ModelPrimitive cube = new(model, PrimitiveType.Cube, _color, Vector3.zero, Vector3.zero);
            cube.GameObject.transform.localScale = new(size, height, 1);
            cube.GameObject.transform.localPosition = new(0, 0, -sizer - 1);
            model.AddPart(cube);
        }
        {
            ModelPrimitive cube = new(model, PrimitiveType.Cube, _color, Vector3.zero, Vector3.zero);
            cube.GameObject.transform.localScale = new(size, height, 1);
            cube.GameObject.transform.localPosition = new(0, 0, sizer + 1);
            model.AddPart(cube);
        }
        {
            ModelPrimitive cube = new(model, PrimitiveType.Cube, _color, Vector3.zero, Vector3.zero);
            cube.GameObject.transform.localScale = new(1, height, size);
            cube.GameObject.transform.localPosition = new(sizer + 1, 0, 0);
            model.AddPart(cube);
        }
        {
            ModelPrimitive cube = new(model, PrimitiveType.Cube, _color, Vector3.zero, Vector3.zero);
            cube.GameObject.transform.localScale = new(1, height, size);
            cube.GameObject.transform.localPosition = new(-sizer - 1, 0, 0);
            model.AddPart(cube);
        }

        {
            ModelPrimitive cube = new(model, PrimitiveType.Cube, _color, Vector3.zero, Vector3.zero);
            cube.GameObject.transform.localScale = new(size, 0.1f, size);
            cube.GameObject.transform.localPosition = new(0, -(height / 2), 0);
            model.AddPart(cube);
            _mainCube = cube;
        }
        {
            ModelPrimitive cube = new(model, PrimitiveType.Cube, _color, Vector3.zero, Vector3.zero);
            cube.GameObject.transform.localScale = new(size, 0.1f, size);
            cube.GameObject.transform.localPosition = new(0, (height / 2), 0);
            model.AddPart(cube);
        }

        Debug.Log(Cells.Count);
    }

    void ProcessWall(Cell cell)
    {
        int x = cell.X;
        int z = cell.Z;
        if (cell.From == null)
        {
            if (Random.Range(0, 2) == 0)
            {
                x += Random.Range(0, 2) - 1;
            }
            else
            {
                z += Random.Range(0, 2) - 1;
            }
        }
        else
        {

            x += (cell.X - cell.From.X);
            z += (cell.Z - cell.From.Z);
        }
        Cell next = GetCellAt(x, z);
        if (next == null || !next.Wall)
            return;
        cell.Wall = false;
        next.Wall = false;


        foreach (Cell process in GetWallsAroundCell(next))
        {
            process.From = next;
            Walls.Add(process);
        }

        Walls.Remove(cell);

    }

    Cell GetCellAt(int x, int z)
    {
        foreach (Cell cell in Cells)
        {
            if (cell.X == x && cell.Z == z)
                return cell;
        }
        return null;
    }

    HashSet<Cell> GetWallsAroundCell(Cell cell)
    {
        HashSet<Cell> near = new();
        HashSet<Cell> check = new()
        {
            GetCellAt(cell.X + 1, cell.Z),
            GetCellAt(cell.X - 1, cell.Z),
            GetCellAt(cell.X, cell.Z + 1),
            GetCellAt(cell.X, cell.Z - 1)
        };

        foreach (Cell checking in check)
        {
            if (checking != null && checking.Wall)
                near.Add(checking);
        }

        return near;

    }


    public class Cell
    {
        public int X { get; set; }

        public int Z { get; set; }

        public bool Wall { get; set; }

        public Cell From { get; set; }

        public Cell(int x, int z)
        {
            X = x;
            Z = z;
            Wall = true;
        }


    }

    static internal (HashSet<ModelPrimitive>, ModelPrimitive) Create(Model model)
    {
        Mazer mazer = new();
        mazer.MazeIt(model);
        return (mazer._escapes, mazer._mainCube);
    }
}