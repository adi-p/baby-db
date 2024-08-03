import './App.css';
import { useState } from 'react'

const HOST = "http://localhost:5000";


function StoreForm() {
  const [key, setKey] = useState("");
  const [value, setValue] = useState("");

  const postStore = async (key, value) => {
    console.log(`Posting Key: ${key} and Value: ${value}`);

    try {
      let response = await fetch(`${HOST}/values`, {
        method: 'POST',
        body: JSON.stringify({
          key,
          value
        })
      });
      // TODO: if successful, store in history
      console.log(response)
    } catch(err) {
      console.log(err);
      // TODO: display error
    }

  }

  const handleStoreSubmit = (e) => {
    e.preventDefault();
    postStore(key, value);
  }

  const clearForm = () => {
    setKey("");
    setValue("");
  }

  return (        
    <div>
      <h2> Store Something </h2>
      <form onSubmit={handleStoreSubmit}>
        <label htmlFor="store-key-input"> Key: </label>
        <input id="store-key-input" type="text" value={key} onChange={(e) => setKey(e.target.value)}></input>
        <label htmlFor="store-value-input"> Value: </label>
        <input id="store-value-input" type="text" value={value} onChange={(e) => setValue(e.target.value)}></input>
        <button type="button" onClick={clearForm}> Clear </button>
        <button type="submit"> Submit </button>
      </form>
    </div>
  );
}

function FetchForm() {
  const [key, setKey] = useState("");
  const [keyValues, setKeyValues] = useState([]);

  const fetchValue = async (key) => {
    console.log(`Fetching value for Key: ${key}`);

    if(!key) {
      return;
    }

    try {
      let response = await fetch(`${HOST}/values/${key}`);
      let data = await response.json();
      if(data.value){
        setKeyValues([{ key, value: data.value }, ...keyValues])
      }
    } catch(err) {
      console.log(err);
      // TODO: display error
    }

  }

  const handleFetchSubmit = (e) => {
    e.preventDefault();
    fetchValue(key);
  }

  const clearForm = () => {
    setKey("");
  }

  return (        
    <div>
      <h2> Fetch Something </h2>
      <form onSubmit={handleFetchSubmit}>
        <label htmlFor="fetch-key-input"> Key: </label>
        <input id="fetch-key-input" type="text" value={key} onChange={(e) => setKey(e.target.value)}></input>
        <button type="button" onClick={clearForm}> Clear </button>
        <button type="submit"> Search </button>
      </form>

      <div>
        <h3> Fetch history </h3>
        <ul>
          {
            keyValues.map((keyValue, index) => {
              return (
                <li key={`${keyValue.key}-${keyValue.value}`}> Value for key <b> {keyValue.key } </b> is : { keyValue.value } </li>
              )
            })
          }
        </ul>
      </div>
    </div>
    
  );
}

function FetchAllForm() {
  const [keyValues, setKeyValues] = useState([]);

  const fetchKeyValues = async () => {
    console.log(`Fetching All key value pairs`);

    try {
      let response = await fetch(`${HOST}/values`);

      let data = await response.json();
      setKeyValues([...data])
    } catch(err) {
      console.log(err);
      // TODO: display error
    }

  }

  const handleFetchAllSubmit = (e) => {
    e.preventDefault();
    fetchKeyValues();
  }

  return (
    <div>
      <h2> Fetch all keys and values </h2>
      <form onSubmit={handleFetchAllSubmit}>
        <button type="submit"> Fetch All </button>
      </form>

      <div>
        <h3> Fetch history </h3>
        <ul>
          {
            keyValues.map((keyValue, index) => {
              return (
                <li key={`${keyValue.key}-${keyValue.value}`}> Value for key <b> {keyValue.key } </b> is : { keyValue.value } </li>
              )
            })
          }
        </ul>
      </div>
    </div>
    
  );
}


function App() {

  return (
    <div className="App">
      <header className="App-header">

        <h1> BabyDb </h1>
        
        <StoreForm></StoreForm>
        <FetchForm></FetchForm>
        <FetchAllForm></FetchAllForm>

      </header>
    </div>
  );
}

export default App;
