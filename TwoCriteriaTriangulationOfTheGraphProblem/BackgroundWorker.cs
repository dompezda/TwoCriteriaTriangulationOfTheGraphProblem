﻿using LiveCharts;
using LiveCharts.Wpf;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using TwoCriteriaTriangulationOfTheGraphProblem.GraphMethods;
using TwoCriteriaTriangulationOfTheGraphProblem.UserControls;

namespace TwoCriteriaTriangulationOfTheGraphProblem
{
    public class BackgroundWorker
    {
        public readonly System.ComponentModel.BackgroundWorker worker;//to służy do wykonywania naprawy grafu w odzielnym wątku
        private Parameters _parameters { get; set; }
        private GeneticAlgorithmMethods geneticAlgorithm;

        public BackgroundWorker(Parameters parameters)
        {
            _parameters = parameters;
            worker = new System.ComponentModel.BackgroundWorker();

            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.ProgressChanged += worker_ProgressChanged;

            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
        }

        private void worker_Report()
        {
            worker.ReportProgress(0);
            _parameters.IterationNumber++;
            Thread.Sleep(_parameters.SleepTime * 1000);
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Aktualizujemy frontend

            //Example graph from AI
            GraphGenerationMethods graphGenerator = new GraphGenerationMethods(_parameters);
            graphGenerator.GenerateTriangulationOfGraph();

            //Refresh pareto front
            var cutsSum = EdgeMethod.GetCutsWeightsSum(_parameters.GeneratedBasicGraph, _parameters.Population);
            var paretoArray = _parameters.FitnessArray.
                Zip(cutsSum, (FitnessAll, second) => (FitnessAll, second)).
                Select(x => new double[] { x.FitnessAll, x.second }).ToList();
            _parameters.RewriteThePoints(paretoArray);
            _parameters.MainWindow.ParetoChart.EditSeriesCollection(_parameters.ListOfPoints);

            //aktualizacja macierzy incydencji
            MatrixMethod matrixMethod = new MatrixMethod(_parameters);

            _parameters.incidenceMatrix = matrixMethod.FillTheSecondHalfOfTheMatrix(_parameters.incidenceMatrix);

            _parameters.GeneratedBasicGraph = EdgeMethod.GenerateEdges(_parameters.incidenceMatrix, _parameters.verticesBasicGeneratedGraph, _parameters.GeneratedBasicGraph);


            //aktualizacja macierzy wag
            //TODO

            //aktualizacja wyświetlanej macierzy incydencji i wag (frontend)
            VertexMethod.CalculateTheSum(_parameters.incidenceMatrix, _parameters.verticesTriangulationOfGraph);
            VertexMethod.SetVertexNeighbors(_parameters.incidenceMatrix, _parameters.verticesTriangulationOfGraph);


            _parameters.MainWindow.OverallFluctuationChart.EditSeriesCollection(_parameters.FitnessArray.Min(), _parameters.IterationNumber);

            _parameters.MainWindow.ProgressBar.Value = _parameters.IterationNumber;

            matrixMethod.RefreshMatrixUi(_parameters.TriangulationOfGraph);
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
           //Przebieg algorytmu genetycznego

           geneticAlgorithm = new GeneticAlgorithmMethods();
            geneticAlgorithm.GeneticAlgorithm(_parameters);

            for (int i = 0; i < _parameters.IterationsLimit; i++)
            {
                geneticAlgorithm.OneMoreTime();

                //Kiedy potrzebujemy odświeżyć UI
                this.worker_Report();
            }

            worker.CancelAsync();
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //Metoda zostaje wywołana zawsze po zakończeniu pracy przez BackgroundWorkera
        }

        #region INotifyPropertyChanged Implementation
        //tym w WPF'ie odświeżamy UI
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        #endregion
    }
}
