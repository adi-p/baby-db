import { useState } from 'react'
import { CONFIG } from "../Config";

export default function FetchForm() {
    const [key, setKey] = useState("");
    const [keyValues, setKeyValues] = useState([]);
  
    const fetchValue = async (key) => {
      console.log(`Fetching value for Key: ${key}`);
  
      if(!key) {
        return;
      }
  
      try {
        let response = await fetch(`${CONFIG.HOST}/values/${key}`);
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
      <div className="form-section">
        <form onSubmit={handleFetchSubmit} className="fetch-form">
          <label htmlFor="fetch-key-input"> Key: </label>
          <input id="fetch-key-input" type="text" value={key} onChange={(e) => setKey(e.target.value)}></input>
          <div className="button-group">
            <button type="button" className="secondary-button" onClick={clearForm}> Clear </button>
            <button type="submit" className="primary-button"> Fetch </button>    
          </div>
        </form>
  
        {!!keyValues.length && <div>
          <h3> Fetch history </h3>
          <ul>
            {
              keyValues.map((keyValue, index) => {
                return (
                  <li key={`${keyValue.key}-${keyValue.value}-${index}`}>
                    <pre className='code-block'>
                      <code>
                        { JSON.stringify(keyValue, null, 1) }
                      </code>
                    </pre>
                  </li>
                )
              })
            }
          </ul>
        </div>}
      </div>
      
    );
  }