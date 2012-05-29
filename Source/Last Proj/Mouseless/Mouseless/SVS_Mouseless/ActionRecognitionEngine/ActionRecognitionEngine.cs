using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using FARInterface;
using ARInterface;
using DataAdapter;

namespace ActionRecognitionEngine
{
    public class ActionRecognitionEngine
    {
        private List<IFingerActionRecognizer> _FAR_Plugins = new List<IFingerActionRecognizer>();                // list of available FAR plugins;
        private List<IActionRecognizer> _AR_Plugins = new List<IActionRecognizer>();                            // list of available AR plugins;

        public List<IActionRecognizer> AR_Plugins
        {
            get { return _AR_Plugins; }
            set { _AR_Plugins = value; }
        }

        private const int MAX_GOF = 100;
        private GroupOfFingers[][] _arrGOF = new GroupOfFingers[MAX_GOF][];         //_arrGOF[i][j] = jth GOF of ith step

        private int _CurrentIdx = MAX_GOF - 1;                            // current index in arrGOF

        public int CurrentIdx
        {
            get { return _CurrentIdx; }
            set { _CurrentIdx = value; }
        }
        private int _Count = 0;                                 // number of step had been analyzed

        private int maxIdx = 0;

        public int Count
        {
            get { return _Count; }
            set { _Count = value; }
        }

        private int[][][] _Prev = new int[MAX_GOF][][];            // prev[i][k][j] = previous index of jth finger of kth GOF of ith step

        FARResult[][][][] _arrFingersAction = new FARResult[MAX_GOF][][][];      // _arrFingersAction[i][s][j][k] = kth result (of kth plugin) for jth finger of sth GOF of ith step

        private int _N_FAR_Plugin = 0;                                  // numbers of FAR plugins

        private int _N_AR_Plugin = 0;                                   // numbers of AR plugins

        public int N_AR_Plugin
        {
            get { return _N_AR_Plugin; }
            set { _N_AR_Plugin = value; }
        }

        /// <summary>
        /// Load FAR plugins from folder
        /// </summary>
        /// <param name="path">path to folder content FAR plugins</param>
        public void Load_FAR_Plugins(String Filepath)
        {
            _N_FAR_Plugin = 0;
            foreach (string Filename in Directory.GetFiles(Filepath, "*.dll"))
            {
                Assembly Asm = Assembly.LoadFile(Filename);
                foreach (Type AsmType in Asm.GetTypes())
                {
                    if (AsmType.GetInterface("IFingerActionRecognizer") != null)
                    {
                        IFingerActionRecognizer Plugin = (IFingerActionRecognizer)Activator.CreateInstance(AsmType);
                        _FAR_Plugins.Add(Plugin);
                        _N_FAR_Plugin++;
                    }
                }
            }
        }

        /// <summary>
        /// Load AR plugins from folder
        /// </summary>
        /// <param name="Filepath">path to folder content AR plugins</param>
        public void Load_AR_Plugins(String Filepath)
        {
            N_AR_Plugin = 0;
            foreach (string Filename in Directory.GetFiles(Filepath, "*.dll"))
            {
                Assembly Asm = Assembly.LoadFile(Filename);
                foreach (Type AsmType in Asm.GetTypes())
                {
                    if (AsmType.GetInterface("IActionRecognizer") != null)
                    {
                        IActionRecognizer Plugin = (IActionRecognizer)Activator.CreateInstance(AsmType);
                        AR_Plugins.Add(Plugin);
                        N_AR_Plugin++;
                    }
                }
            }
        }


        /// <summary>
        /// Insert new groups of fingers
        /// </summary>
        /// <param name="GOF">groups of fingers</param>
        public void InsertGOF(GroupOfFingers[] GOF)
        {
            int lastIdx = CurrentIdx;
            int nextIdx = (CurrentIdx + 1) % MAX_GOF;
            _arrGOF[nextIdx] = GOF;
            CurrentIdx = nextIdx;
            if (maxIdx < CurrentIdx)
            {
                maxIdx = CurrentIdx;
            }
            Count++;

            // xac dinh previous match
            // prev[i][k][j] = previous index of jth finger of kth GOF of ith step
            _Prev[CurrentIdx] = new int[GOF.Length][];

            // duyet qua danh sach GOF

            // cach naive
            for (int i = 0; i < GOF.Length; i++)
            {
                _Prev[CurrentIdx][i] = new int[GOF[i].N];
                for (int j = 0; j < GOF[i].N; j++)
                {
                    if (lastIdx  <= maxIdx &&
                        _Prev[lastIdx].Length > i &&
                        j < _Prev[lastIdx][i].Length)
                        _Prev[CurrentIdx][i][j] = j;
                    else
                        _Prev[CurrentIdx][i][j] = -1;
                }
            }

            RecognizeActionForEachFinger();
        }


        /// <summary>
        /// Recognize action for fingers
        /// </summary>
        public void RecognizeActionForEachFinger()
        {
            // duyet danh sach tung GOF
            _arrFingersAction[CurrentIdx] = new FARResult[_arrGOF[CurrentIdx].Length][][];
            for (int k = 0; k < _arrGOF[CurrentIdx].Length; k++)
            {
                _arrFingersAction[CurrentIdx][k] = new FARResult[_arrGOF[CurrentIdx][k].N][];
                int cnt = 0;
                for (int i = 0; i < _arrGOF[CurrentIdx][k].N; i++)
                {
                    _arrFingersAction[CurrentIdx][k][i] = new FARResult[_N_FAR_Plugin];

                    cnt = 0;
                    foreach (IFingerActionRecognizer Plugin in _FAR_Plugins)
                    {
                        _arrFingersAction[CurrentIdx][k][i][cnt] = Plugin.Recognize(_arrGOF, _Prev, Count, CurrentIdx, i, k);
                        cnt++;
                    }
                }
            }
        }

        /// <summary>
        /// Recognize intended action
        /// </summary>
        public ARResult[] Recognize()
        {
            ARResult[] rsl = new ARResult[N_AR_Plugin];
            // duyet danh sach tung plugins
            int cnt = 0;
            foreach (IActionRecognizer Plugin in AR_Plugins)
            {
                rsl[cnt] = Plugin.Recognize(_arrFingersAction, CurrentIdx, _Prev, _arrGOF[CurrentIdx].Length, _N_FAR_Plugin);

                //if (rsl[cnt].Name == "SURFACE ZOOM ACTION")
                //{
                //}


                cnt++;
            }

            return rsl;
        }

        public string[] GetActionsName()
        {
            string[] rsl = new string[N_AR_Plugin];
            int cnt = 0;
            foreach (IActionRecognizer Plugin in AR_Plugins)
            {
                rsl[cnt] = Plugin.GetName();
                cnt++;
            }
            return rsl;
        }

        public void GetCurrentFingertips3D(double[] p3Dx, double[] p3Dy, double[] p3Dz, ref int n)
        {
            n = 0;
            // duyet qua cac tap hop dau ngon tay
            for (int i = 0; i < _arrGOF[CurrentIdx].Length; i++)
            {
                // duyet qua cac dau ngon tay
                for (int j = 0; j < _arrGOF[CurrentIdx][i].N; j++)
                {
                    p3Dx[n] = _arrGOF[CurrentIdx][i].Fingertips[j].Point3D.X;
                    p3Dy[n] = _arrGOF[CurrentIdx][i].Fingertips[j].Point3D.Y;
                    p3Dz[n] = _arrGOF[CurrentIdx][i].Fingertips[j].Point3D.Z;
                    n++;
                }
            }
        }

        public void reset()
        {
            CurrentIdx = MAX_GOF - 1;
            Count = 0;
        }

        public string[] getState()
        {
            int fCnt = 0;
            for (int k = 0; k < _arrGOF[CurrentIdx].Length; k++)
            {
                fCnt += _arrGOF[CurrentIdx][k].N;
            }

            string[] rsl = new string[fCnt];
            
            fCnt = 0;
            for (int k = 0; k < _arrGOF[CurrentIdx].Length; k++)
            {
                for (int i = 0; i < _arrGOF[CurrentIdx][k].N; i++)
                {
                    rsl[fCnt] = "";
                    for (int cnt = 0; cnt < _N_FAR_Plugin; cnt++)
                    {
                        rsl[fCnt] += _arrFingersAction[CurrentIdx][k][i][cnt].Name;
                        rsl[fCnt] += ":";
                        cnt++;
                    }
                    rsl[fCnt] += "  ";
                    fCnt++;
                }
            }

            return rsl;
        }
    }
}