const Nav = () => {
  return (
    <div className="flex items-center justify-between w-[90%] mx-auto my-0 font-inter">
      <h2 className="font-bold text-[20px] md:text-2xl leading-[100%]">Jsonizelt</h2>
      <a href="#how-it-works" className="font-medium text-[12px] md:text-[14px] leading-[100%]">How it Works</a>
    </div>
  )
}

const Header = () => {
  return (
    <div className="font-inter flex flex-col items-center gap-10 text-white bg-[#001941] pt-3 min-h-[30vh] md:min-h-[33vh]">
      <Nav />
      <div className="flex flex-col space-y-3 text-center px-4 md:px-0">
        <h3 className="font-bold text-2xl md:text-[32px] leading-[100%]">From code to JSON in one click.</h3>
        <p className="font-medium text-[14px] md:text-[16px] leading-[100%]">Select file type, upload or paste text, click convert to get your JSON output</p>
      </div>
    </div>
  )
}

export default Header
