using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QuestPDF.Drawing;
using QuestPDF.Drawing.Exceptions;
using QuestPDF.Infrastructure;

namespace QuestPDF.Previewer
{
    public static class PreviewerExtensions
    {
        #if NET6_0_OR_GREATER
        
        /// <include file='../Resources/Documentation.xml' path='documentation/doc[@for="previewer.supported"]/*' />
        public static void ShowInPreviewer(this IDocument document, int port = 12500)
        {
            document.ShowInPreviewerAsync(port).ConfigureAwait(true).GetAwaiter().GetResult();
        }
        
        /// <include file='../Resources/Documentation.xml' path='documentation/doc[@for="previewer.supported"]/*' />
        public static async Task ShowInPreviewerAsync(this IDocument document, int port = 12500, CancellationToken cancellationToken = default)
        {
            // TODO: find better way to disable caching?
            Settings.EnableCaching = false;
            
            var previewerService = new PreviewerService(port);
            
            using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            previewerService.OnPreviewerStopped += () => cancellationTokenSource.Cancel();

            await previewerService.Connect();
            previewerService.StartRenderRequestedPageSnapshotsTask(cancellationToken);
            await RefreshPreview();
            
            HotReloadManager.UpdateApplicationRequested += (_, _) => RefreshPreview();
            
            await KeepApplicationAlive(cancellationTokenSource.Token);
            
            Task RefreshPreview()
            {
                try
                {
                    var pictures = DocumentGenerator.GeneratePreviewerContent(document);
                    return previewerService.RefreshPreview(pictures);
                }
                catch (DocumentLayoutException documentLayoutException)
                {
                    return previewerService.InformAboutLayoutError(documentLayoutException.PreviewCommand);
                }
                catch (Exception exception)
                {
                    return previewerService.InformAboutGenericException(exception);
                }
            }

            async Task KeepApplicationAlive(CancellationToken cancellationToken)
            {
                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;
                
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }
            }
        }
        
        #else

        /// <include file='../Resources/Documentation.xml' path='documentation/doc[@for="previewer.notSupported"]/*' />
        public static void ShowInPreviewer(this IDocument document, int port = 12500)
        {
            throw new Exception("The hot-reload feature requires .NET 6 or later.");
        }

        /// <include file='../Resources/Documentation.xml' path='documentation/doc[@for="previewer.notSupported"]/*' />
        public static async Task ShowInPreviewerAsync(this IDocument document, int port = 12500, CancellationToken cancellationToken = default)
        {
            throw new Exception("The hot-reload feature requires .NET 6 or later.");
        }

        #endif
    }
}
