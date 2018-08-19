using MeshGenerator._3dObjectSettings;
using MeshGenerator.Elements;
using MeshGenerator.Model;
using MeshGenerator.Triangulation;
using MeshGenerator.Volume;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Volot.Model
{
    public class ModelGeneration
    {
        private string path;
        private int step;
        IRepository<string, List<Node>> repository;

        #region Properties
        /// <summary>
        /// Finite element model
        /// </summary>
        public FeModel Model { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Generation finite element model
        /// </summary>
        /// <param name="path">Path to directory with DICOM data</param>
        /// <param name="step">Step of the mesh</param>
        public ModelGeneration(string path, int step = 10)
        {
            this.step = step;
            this.path = path;
            //repository = new ObjRepository<string>();
            repository = new XyzRepository<string>();

            Model = Generation();
        }

        /// <summary>
        /// Generation finite element model
        /// </summary>
        /// <param name="model">Finite element model</param>
        public ModelGeneration(FeModel model)
        {
            Model = model;
        }
        #endregion

        #region Methods

        List<List<Node>> InitLayers(List<Node> nodes)
        {
            List<List<Node>> layers = new List<List<Node>>();
            nodes.Sort((nd1, nd2) =>
            {
                return (nd1.PZ > nd2.PZ)
                ? 1
                : (nd1.PZ < nd2.PZ) ? -1 : 0;
            });
            int startZ = nodes[0].PZ;
            int endZ = nodes[nodes.Count - 1].PZ;
            for (int pz = startZ; pz < endZ; pz++)
            {
                List<Node> tmpList = new List<Node>();
                tmpList.AddRange(nodes.Where(node => node.PZ == pz).ToList());
                if (tmpList.Count > 0)
                {
                    layers.Add(tmpList);
                    nodes.RemoveAll(node => node.PZ == pz);
                }
            }
            return layers;
        }
        /// <summary>
        /// Gneration of the finite element model
        /// </summary>
        /// <returns></returns>
        FeModel Generation()
        {
            FeModel result = null;

            Dictionary<int, List<Area>> items = new Dictionary<int, List<Area>>();
            Dictionary<int, List<List<Triangle>>> trianglesLayers = new Dictionary<int, List<List<Triangle>>>();
            List<Tetrahedron> tetrahedrons = new List<Tetrahedron>();
            List<Node> nodes = new List<Node>();

            Triangulating triangulate = new Triangulating(step);
            VolumeGeneration volume = new VolumeGeneration(step);

            List<List<Node>> boundLayers = InitLayers(repository.Read(path));

            int difference = step / Math.Abs(boundLayers[1][0].PZ - boundLayers[0][0].PZ);

            for (int i = 0; i < boundLayers.Count - difference; i++)
            {
                boundLayers.RemoveRange(i + 1, difference - 1);
            }

            int processCount = boundLayers.Count;
            int counter = 0;
            using (ManualResetEvent resetEvent = new ManualResetEvent(false))
            {
                foreach (var item in boundLayers)
                {
                    ThreadPool.SetMaxThreads(4, 8); // 8 because 4 core and 4 virtual
                    ThreadPool.QueueUserWorkItem(thrLayer =>
                    {
                        var layer = (((object[])thrLayer)[0]) as List<Node>;
                        int index = (int)((object[])thrLayer)[1];
                        LayerSettings settings = new LayerSettings(layer, step);
                        List<Area> areas = settings.Areas;
                        
                        items.Add(index, areas);
                        List<List<Triangle>> triangles = new List<List<Triangle>>();
                        areas.ForEach(area =>
                        {
                            List<Triangle> tmp = triangulate.GenerateTriangles(area.Nodes).Values.ToList();
                            if (tmp.Count > 0) triangles.Add(tmp);
                        });
                        trianglesLayers.Add(index, triangles);

                        if (Interlocked.Decrement(ref processCount) == 0)
                        {
                            resetEvent.Set();
                        }
                    }, new object[] { item, counter++ });
                }
                resetEvent.WaitOne();
            }
            for (int i = 0; i < items.Count; i++)
            {
                items[i].ForEach(ar => nodes.AddRange(ar.Nodes));
            }
            //nodes.Sort((first, second) =>
            //{
            //    return (first.Z > second.Z) ? 1
            //    : (first.Z < second.Z) ? -1
            //    : 0;
            //});
            // set global indexes by getting from current area id plus count elements in previous areas
            // after generate triangles and tetrahedrons (try with thread pool)
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].GlobalIndex = i;
            }

            //tetrahedron generation
            tetrahedrons = GenerateTetrahedrons(ref trianglesLayers, volume, ref items);

            result = new FeModel(nodes, tetrahedrons);
            return result;
        }

        /// <summary>
        /// Tetrahedrons generation
        /// </summary>
        /// <param name="trianglesLayers"> Layers of triangles</param>
        /// <param name="volume">Volume generation settings</param>
        /// <param name="areas">List of areas by layers</param>
        /// <returns></returns>
        List<Tetrahedron> GenerateTetrahedrons(ref Dictionary<int, List<List<Triangle>>> trianglesLayers, VolumeGeneration volume, ref Dictionary<int, List<Area>> areas)
        {
            List<Tetrahedron> tetrahedrons = new List<Tetrahedron>();
            for (int i = 1; i < trianglesLayers.Count; i++)
            {
                if (trianglesLayers[i - 1].Count > 0 && trianglesLayers[i].Count > 0)
                {
                    if (trianglesLayers[i - 1].Count < trianglesLayers[i].Count)
                        tetrahedrons.AddRange(volume.GenerateTetrahedrons(trianglesLayers[i - 1], trianglesLayers[i]));
                    else
                        tetrahedrons.AddRange(volume.GenerateTetrahedrons(trianglesLayers[i], trianglesLayers[i - 1]));

                }
                else
                {
                    if (trianglesLayers[i - 1].Count < trianglesLayers[i].Count)
                    {
                        List<Triangle> tmp = new List<Triangle>();
                        trianglesLayers[i].ForEach(x => tmp.AddRange(x));
                        tetrahedrons.AddRange(volume.GenerateTetrahedrons(areas[i - 1][0].Nodes[0], tmp));
                    }
                    else
                    {
                        List<Triangle> tmp = new List<Triangle>();
                        trianglesLayers[i - 1].ForEach(x => tmp.AddRange(x));
                        tetrahedrons.AddRange(volume.GenerateTetrahedrons(areas[i][0].Nodes[0], tmp));
                    }

                }
            }
            return tetrahedrons;
        }
        #endregion
    }
}
