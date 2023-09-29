using Firebase.Auth;
using Firebase.Storage;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implement {
    public class ImageService : IImageService {
        private readonly IConfiguration _config;

        public ImageService(IConfiguration config) {
            _config = config;
        }

        public async Task<string> StoreImageAsync(string fileName, Stream stream) {
            var auth = new FirebaseAuthProvider(new FirebaseConfig(_config["Firebase:ApiKey"]));
            var a = await auth.SignInWithEmailAndPasswordAsync(_config["Firebase:AuthEmail"], _config["Firebase:AuthPassword"]);

            // Constructr FirebaseStorage, path to where you want to upload the file and Put it there
            var task = new FirebaseStorage(
                _config["Firebase:Bucket"],

                 new FirebaseStorageOptions {
                     AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                     ThrowOnCancel = true,
                 })
                .Child("img")
                .Child("avt")
                .Child(fileName)
                .PutAsync(stream);

            // Track progress of the upload
            //task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");

            // await the task to wait until upload completes and get the download url
            var downloadUrl = await task;
            return downloadUrl;
        }
    }
}
