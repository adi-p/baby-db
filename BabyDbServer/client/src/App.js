import './App.css';
import { useRef, useEffect } from 'react'
import StoreForm from './components/StoreForm';
import FetchForm from './components/FetchForm';
import DeleteForm from './components/DeleteForm';
import FetchAllForm from './components/FetchAllForm';

function App() {
  const storeFormRef = useRef(null);
  
  useEffect(() => {
    storeFormRef.current?.focus()
  }, [storeFormRef])

  return (
    <div className="App">
      <header className="App-header">
        <h1> BabyDb </h1>
      </header>

      <div className="body-rows">
        <div className="row">
          <StoreForm ref={storeFormRef} onClick=""></StoreForm>
          <FetchForm></FetchForm>
          <DeleteForm></DeleteForm>
        </div>
        <div className="row">
          <FetchAllForm></FetchAllForm>
        </div>
      </div>
    </div>
  );
}

export default App;
