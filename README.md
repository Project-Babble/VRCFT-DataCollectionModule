# VRCFT-DataCollectionModule
The `DataCollectionModule` is a module for the VRCFaceTracking framework designed to collect facial expression data, eye images, and face images at specified intervals. This module writes the collected data to a CSV file for further analysis or processing.

## Features
- Collects data for all or specific expressions defined in `UnifiedExpressions`.
- Supports optional eye and face image capture.
- Configurable data collection interval.
- Appends data to an existing CSV file or creates a new one.
- Includes metadata like image dimensions and Base64-encoded image data.

## Installation

Download the module from the module registry within VRCFaceTracking.

## Usage

### 1. **Configuration**
The `DataCollectionConfig.json` file defines how the module operates. Modify the file as needed before starting the application.

You can find the file under `%AppData%\Roaming\VRCFaceTracking\CustomLibs\{ModuleHash}\DataCollectionConfig.json`

#### Sample Configuration:
```json
{
  "FilePath": "data.csv",
  "CollectionInterval": 200,
  "ExpressionFilter": [],
  "CaptureEyeImages": true,
  "CaptureFaceImages": true,
  "Append": false
}
```

- **FilePath**: Path to the CSV file where data is stored.  
- **CollectionInterval**: Interval in milliseconds between data captures.  
- **ExpressionFilter**: Specify a subset of expressions to record. Leave empty to record all.  
- **CaptureEyeImages**: Enable/disable eye image capture.  
- **CaptureFaceImages**: Enable/disable face image capture.  
- **Append**: Append data to an existing file or overwrite it.

### 2. **Starting the Module**
Once configured, the module initializes automatically when the application starts if properly loaded by `VRCFaceTracking`.

### 3. **Collected Data Format**
The CSV file contains:
- One column for each expression in `UnifiedExpressions`.
- Image metadata (if enabled):
  - `EyeImageX`, `EyeImageY`, and Base64-encoded `EyeImageData`.
  - `FaceImageX`, `FaceImageY`, and Base64-encoded `FaceImageData`.

#### Example Output:
| Expression1 | Expression2 | EyeImageX | EyeImageY | EyeImageData | FaceImageX | FaceImageY | FaceImageData |
|-------------|-------------|-----------|-----------|--------------|------------|------------|--------------|
| 0.5         | 0.7         | 64        | 64        | ...Base64... | 128        | 128        | ...Base64... |

## License
This module is released under the MIT License. See the LICENSE file for details.

## Credits
All credits to [FlatIcon](https://www.flaticon.com/free-icons/spreadsheet) for the spreadsheet icon.
