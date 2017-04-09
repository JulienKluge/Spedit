using SourcepawnCondenser;
using SourcepawnCondenser.SourcemodDefinition;
using System.Threading;
using System.Timers;
using System.IO;
using Spedit.UI.Components;

namespace Spedit.UI
{
	public partial class MainWindow
	{
        private Thread _backgroundParserThread;
        private SMDefinition _currentSMDef;
        private System.Timers.Timer _parseDistributorTimer;

        public ulong CurrentSMDefUID;
        public SMFunction[] CurrentSMFunctions;
        public AcNode[] CurrentAcNodes;
        public IsNode[] CurrentIsNodes;

		public void StartBackgroundParserThread()
		{
			_backgroundParserThread = new Thread(BackgroundParser_Worker);
			_backgroundParserThread.Start();
			_parseDistributorTimer = new System.Timers.Timer(500.0);
			_parseDistributorTimer.Elapsed += ParseDistributorTimer_Elapsed;
			_parseDistributorTimer.Start();
		}

		private void ParseDistributorTimer_Elapsed(object sender, ElapsedEventArgs args)
		{
		    if (CurrentSMDefUID == 0)
		        return;

			EditorElement[] ee = null;
			EditorElement ce = null;

			Dispatcher.Invoke(() =>
			{
				ee = GetAllEditorElements();
				ce = GetCurrentEditorElement();
			});

		    if (ee == null || ce == null)
		        return;

			foreach (var e in ee)
			{
			    if (e.LastSMDefUpdateUID >= CurrentSMDefUID)
                    continue;

			    if (e == ce)
			        if (ce.IsacOpen)
			            continue;

			    e.InterruptLoadAutoCompletes(_currentSMDef.FunctionStrings, CurrentSMFunctions, CurrentAcNodes, CurrentIsNodes);
			    e.LastSMDefUpdateUID = CurrentSMDefUID;
			}
		}
		
		private void BackgroundParser_Worker()
		{
			while (true)
			{
				while (Program.OptionsObject.ProgramDynamicIsac)
				{
					Thread.Sleep(5000);
					var ee = GetAllEditorElements();

				    if (ee == null)
                        continue;

				    var definitions = new SMDefinition[ee.Length];

				    for (var i = 0; i < ee.Length; ++i)
				    {
				        var fInfo = new FileInfo(ee[i].FullFilePath);

				        if (fInfo.Extension.Trim('.').ToLowerInvariant() != "inc")
                            continue;
				        
				        definitions[i] = new Condenser(File.ReadAllText(fInfo.FullName), fInfo.Name).Condense();
				    }

				    _currentSMDef = (Program.Configs[Program.SelectedConfig].GetSMDef()).ProduceTemporaryExpandedDefinition(definitions);
				    CurrentSMFunctions = _currentSMDef.Functions.ToArray();
				    CurrentAcNodes = _currentSMDef.ProduceAcNodes();
				    CurrentIsNodes = _currentSMDef.ProduceIsNodes();
				    ++CurrentSMDefUID;
				}

				Thread.Sleep(5000);
			}
		}
	}
}
