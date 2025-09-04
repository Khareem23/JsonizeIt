import { useState, useCallback, useRef } from "react";
// import { jsonConverter } from "../jsonConverter";
import copy from "../assets/images/copy-icon.svg";
import download from "../assets/images/download-icon.svg";
import feature from "../assets/images/Featured icon.svg";
import onedriveIcon from "../assets/images/onedrive.svg"
import fileIcon from "../assets/images/fileicon.svg"

const programTypes = [
 "C#",
  "TypeScript",
  "Java",
  "Dart",
  "Python",
  "XML",
  "CSV"
];

const Hero = () => {
  const [inputMode, setInputMode] = useState<"text" | "file">("text");
  const [textInput, setTextInput] = useState("");
  const [jsonOutput] = useState("");
  const [fileName, setFileName] = useState("");
  const [isDragActive, setIsDragActive] = useState(false);
   const [language, setLanguage] = useState(programTypes[0]);
  const [pascalCase, setPascalCase] = useState(false);
  const [nullable, setNullable] = useState(false);
  const [loading, setLoading] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const processFile = useCallback((file: File) => {
    if (!file) return;

    const allowedExtensions = [ ".cs", 
      ".ts",   
      ".java",
      ".dart",
      ".py",
      ".xml",
      ".csv" ];
    const fileExtension = '.' + file.name.split('.').pop()?.toLowerCase();
    
    if (!allowedExtensions.includes(fileExtension)) {
      alert('Please select a valid file type');
      return;
    }

    setFileName(file.name);

  const reader = new FileReader();
  reader.onload = async () => {
    const fileContent = reader.result as string;
    setTextInput(fileContent); 

    try {
      setLoading(true);
      // const data = await convertCode(fileContent); 
      // setJsonOutput(JSON.stringify(data, null, 2));
    } catch (err) {
      console.error(err);
      alert("Conversion failed.");
    } finally {
      setLoading(false);
    }
  };
  reader.readAsText(file);
}, []);

  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      processFile(file);
    }
  };

  const handleDragEnter = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragActive(true);
  };

  const handleDragLeave = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragActive(false);
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragActive(false);

    const files = e.dataTransfer.files;
    if (files && files[0]) {
      processFile(files[0]);
    }
  };

  const handleClick = () => {
    fileInputRef.current?.click();
  };

const handleConvert = async () => {
  console.log("converted")
};

  const handleCopy = () => {
    navigator.clipboard.writeText(jsonOutput);
  };

  const handleDownload = () => {
    const blob = new Blob([jsonOutput], { type: "application/json" });
    const link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = fileName || "output.json";
    link.click();
  };

  return (
    <section className="flex flex-col md:flex-row bg-white items-stretch justify-start md:justify-between rounded-4xl font-inter gap-0 md:gap-4 border-4 border-[#00145299] w-[90%] md:w-[80%] lg:w-[70%] mx-auto -my-[74px] mb-8">      
      {/* LEFT SIDE */}
      <div className="w-full md:w-1/2 min-h-[620px] flex flex-col flex-1 items-stretch border-[#B9B9B9] md:border-r-2">

        <div className="flex items-center gap-4 border-[#B9B9B9] border-b-2 border-r-0 border-l-0 px-5 py-4">
          <h4 className="text-[#1E1E1E] font-bold">Input file type</h4>
          <select value={language}
            onChange={(e) => setLanguage(e.target.value)} className="rounded-[8px] border border-[#D0D5DD] p-2 cursor-pointer focus:outline-none w-[30%]">
            {programTypes.map((type) => (
              <option key={type} value={type}>
                {type}
              </option>
            ))}
          </select>
        </div>

        <div className="pt-4">
          <div  className={`border border-[#EAECF0] rounded-[8px] py-4 flex flex-col mx-4 ${
    inputMode === "file" ? "mb-32" : "mb-4"
  }`}>
            <div className="w-full border-[#EAECF0] border-b">
            <div className="bg-[#DFE7FF] flex items-center justify-center text-[#1E1E1E] text-[12px] rounded-[8px] w-[70%] mx-auto mb-4 p-1">
              <p
                className={`cursor-pointer text-center w-1/2 py-2 ${inputMode === "text" ? "bg-white rounded-[4px]" : ""}`}
                onClick={() => setInputMode("text")}
              >
                Text Input
              </p>
              <p
                className={`cursor-pointer text-center w-1/2 py-2 ${inputMode === "file" ? "bg-white rounded-[4px]" : ""}`}
                onClick={() => setInputMode("file")}
              >
                Doc Upload
              </p>
            </div>
            </div>
            {inputMode === "text" ? (
              <textarea
                className=" rounded-[8px] px-3 py-2 w-full resize-none bg-[#FBFBFB] h-[370px] focus:outline-none"
                placeholder="Enter your text here"
                rows={8}
                value={textInput}
                onChange={(e) => setTextInput(e.target.value)}
              />
            ) : (
              <div>
                <input
                  ref={fileInputRef}
                  type="file"
                  accept=".json,.txt,.py,.java,.cpp,.cs,.go,.rs,.rb,.php,.js,.ts"
                  onChange={handleFileChange}
                  style={{ display: 'none' }}
                />
                
                <div
                  onDragEnter={handleDragEnter}
                  onDragLeave={handleDragLeave}
                  onDragOver={handleDragOver}
                  onDrop={handleDrop}
                  className={`px-8 py-16 text-center rounded-[8px] transition-colors w-full ${
                    isDragActive 
                      ? "border-[#0037DD] bg-blue-50" 
                      : "border-[#E5E7EB]"
                  }`}
                >
                  <div className="mb-4">
                <img src={feature} alt="drag and drop icon" className="mx-auto my-0" />
                  </div>
                  
                  {isDragActive ? (
                    <p className="text-[#0037DD] text-sm">Drop the file here...</p>
                  ) : (
                    <div>
                      <p className="text-[#475467] text-sm leading-5 mb-6">
                        Drag and drop files here or browse your files
                      </p>
                      
                      <div className="flex gap-3 justify-center">
                        <button
                          onClick={(e) => {
                            e.stopPropagation();
                            handleClick();
                          }}
                          className="flex items-center gap-2 px-2 py-1 bg-[#F2F3FF] text-[#0037DD] rounded-[4px] text-sm font-medium transition-colors cursor-pointer"
                        >
                          <img src={fileIcon} alt="browse files icon" />
                          Browse files
                        </button>
                        
                        <button
                          onClick={(e) => {
                            e.stopPropagation();
                            alert('OneDrive integration not implemented');
                          }}
                          className="flex items-center gap-2 px-2 py-1 bg-[#F2F3FF] text-[#0037DD] rounded-[4px] text-sm font-medium transition-colors cursor-pointer"
                        >
                          <img src={onedriveIcon} alt="one drive icon" />
                          From OneDrive
                        </button>
                      </div>
                    </div>
                  )}
                  
                  {fileName && (
                    <div className="mt-4 p-3 bg-green-50 rounded-[6px] border border-green-200">
                      <p className="text-sm text-green-700">✓ Uploaded: {fileName}</p>
                    </div>
                  )}
                </div>
              </div>
            )}
          </div>

          <div className="flex flex-col justify-center border-[#D1D1D1] border-t py-6 px-4 gap-3 mt-auto">
            <h4 className="font-medium text-[#1E1E1E]">Property Settings</h4>
            <div className="flex items-center gap-6">
              <label className="flex items-center gap-2 cursor-pointer">
                <input type="checkbox" checked={pascalCase} onChange={(e) => setPascalCase(e.target.checked)} /> 
                <p className="text-sm text-[#344054]">Use pascal case</p>
              </label>
              <label className="flex items-center gap-2 cursor-pointer">
                <input type="checkbox" checked={nullable} onChange={(e) => setNullable(e.target.checked)}/> 
                <p className="text-sm text-[#344054]">Use nullable values</p>
              </label>
            </div>

            <button
              onClick={handleConvert}
              className="bg-[#0037DD] px-4 py-2.5 text-white text-sm font-semibold w-[30%] mx-auto rounded-[8px] mt-2 cursor-pointer" disabled={loading}
            >
               {loading ? "Converting..." : "Convert"}
            </button>
          </div>
        </div>
      </div>

      {/* RIGHT SIDE - JSON Preview */}
      <div className="w-full md:w-1/2 h-full">
        <div className="flex border-[#B9B9B9] border-b-2 md:border-l-2 px-5 py-[22px]">
          <h4 className="text-[#1E1E1E] font-bold">JSON Output</h4>
        </div>
        <div className="border-[#B9B9B9] md:border-l-2 p-4">
          <div className="bg-[#171717] h-[650px] rounded-br-2xl">
            <div className="flex items-center justify-between py-3 px-4">
              <h4 className="font-bold text-white">JSON</h4>
              <div className="flex items-center gap-4 text-white">
                <div className="flex items-center gap-2.5 px-2 py-1 cursor-pointer" onClick={handleCopy}>
                  <p className="text-[12px]">Copy</p>
                  <img src={copy} alt="copy icon" />
                </div>
                <div className="flex items-center gap-2.5 bg-[#00A86E] px-2 py-1 cursor-pointer" onClick={handleDownload}>
                  <p className="text-[12px] rounded-[4px]">Download</p>
                  <img src={download} alt="download icon" />
                </div>
              </div>
            </div>
            <textarea
              name="json-code"
              id="json-code"
              className="w-full text-white px-4 py-3 h-[90%] border-[#434343] border-t bg-[#171717] resize-none focus:outline-none"
              value={jsonOutput}
              readOnly
              placeholder={`{\n //JSON preview will be displayed here\n}`}
            />
          </div>
        </div>
      </div>
    </section>
  );
};

export default Hero;