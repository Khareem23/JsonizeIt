import clipboardIcon1 from "../assets/images/clipboardIcon.svg"
import convertIcon from "../assets/images/convertIcon.svg"
import clipboardIcon2 from "../assets/images/clipboardIcon2.svg"
import clipboardIcon1White from "../assets/images/clipboardIconWhite.svg"
import convertIconWhite from "../assets/images/convertIconWhite.svg"
import clipboardIcon2White from "../assets/images/clipboardIcon2White.svg"

const HowItWorks = () => {
  const methodData = [
    {
      id: "01",
      icon: clipboardIcon1,
      iconHover: clipboardIcon1White,
      title: "Upload or Paste Text",
      description: "Select file type, upload or paste text, click convert to get your JSON output, Select file type, upload or paste text, click convert to get your JSON output"
    },
    {
      id: "02",
      icon: convertIcon,
      iconHover: convertIconWhite,
      title: "We convert input",
      description: "Select file type, upload or paste text, click convert to get your JSON output, Select file type, upload or paste text, click convert to get your JSON output"
    },
    {
      id: "03",
      icon: clipboardIcon2,
      iconHover: clipboardIcon2White,
      title: "Copy/Download Options",
      description: "Select file type, upload or paste text, click convert to get your JSON output, Select file type, upload or paste text, click convert to get your JSON output"
    },

  ]
  return (
    <section id="how-it-works" className="font-inter w-[90%] md:w-[85%] mx-auto my-0 pb-40 pt-20">
      <div className="flex flex-col items-center justify-center gap-3 mb-16 text-center">
        <h3 className="font-bold text-[32px]">How It Works</h3>
        <p className="text-[#878787] font-medium">Select file type, upload or paste text, click convert to get your JSON output</p>
      </div>
      <div className="flex flex-col md:flex-row items-stretch justify-between gap-5">
        {
          methodData.map((method, index) => (
            <div key={index} className="group flex flex-col items-center justify-center w-full md:w-1/3 mb-4 md:mb-0" >
              <p className="font-bold text-[64px] text-[#D1D1D1] leading-[100%] transition-all duration-300 -mb-8 group-hover:-translate-y-8 group-hover:text-[#001941]">{method.id}</p>
            <div className=" flex flex-col items-start justify-center gap-2.5 bg-white border border-[#B4B3B3] rounded-[4px] p-5 cursor-pointer  h-full transition-all duration-300 group-hover:bg-[#001941] group-hover:text-white group-hover:border-white">
              <img src={method.icon} alt={method.title} className="block transition-all duration-300 group-hover:hidden" />
              <img src={method.iconHover} alt={`${method.title} Hover`} className="hidden  transition-all duration-300 group-hover:block" />
              <h4 className="font-medium text-[20px] text-[#1E1E1E] leading-[150%] group-hover:text-white">{method.title}</h4>
              <p className="text-[#878787] font-medium leading-[150%] group-hover:text-white">{method.description}</p>
            </div>
            </div>
          ))
        }
      </div>
    </section>
  )
}

export default HowItWorks
