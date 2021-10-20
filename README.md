# DownloadWorkerPlatform

The system provides a way to download a large file on local computer via https protocol.

## Components

There are two components:

- *DownloadService*: it's the backend service with a download endpoint
- *DownloadWorker*: it's the local service for downloading from DownloadService.

The DownloadService reads the file to download specified in the appsettings.json. Once the client request through the download endpoint, the service provides it.

The DownloadWorker has the download service to reach the endpoint end request to download.

There are two implementation:

- DownloadService: it's a custom implementation with the possibility to specify the number of parallel downloads. A feature good to be implemented could be the resume functionality.

- AZCopyDownloadService: it's a wrapper around zacopy utility. The resume is a native feature. It works only for Azure Storage Service.

*Note*: for the *azcopy* implementation, it's necessary to download the utility (<https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azcopy-v10>), put it in the DownloadWorker root folder and set to copy in the output.

## References

### Resume

- <https://stackoverflow.com/questions/15706322/how-to-resume-broken-download-between-a-c-sharp-server-and-java-client>
- <https://codereview.stackexchange.com/questions/97872/resumable-http-download-class>
- <https://gist.github.com/stormwild/040d446d398c158db09cfc072c4cb4ab>
- <https://gist.github.com/blueblip/f9c8284023e627089c3f06aa2e06de3e>
- <https://docs.microsoft.com/en-us/azure/storage/common/storage-ref-azcopy-jobs-resume>

### Chunks

- <https://dejanstojanovic.net/aspnet/2018/march/download-file-in-chunks-in-parallel-in-c/>

### AZCopy

- <https://docs.microsoft.com/en-us/azure/storage/common/storage-ref-azcopy-copy>