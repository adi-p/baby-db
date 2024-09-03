import { useState } from "react";
import { CONFIG } from '../Config';


export default function DeleteForm() {
    const [key, setKey] = useState("");
  
    const deleteValue = async (key) => {
      console.log(`delete value for Key: ${key}`);
  
      if(!key) {
        return;
      }
  
      try {
        let response = await fetch(`${CONFIG.HOST}/values/${key}`, { method: 'DELETE' });
        let data = await response.json();
        console.log(data);
      } catch(err) {
        console.log(err);
        // TODO: display error
      }
  
    }
  
    const handleDeleteSubmit = (e) => {
      e.preventDefault();
      deleteValue(key);
    }
  
    const clearForm = () => {
      setKey("");
    }
  
    return (        
      <div className="form-section">
        <form onSubmit={handleDeleteSubmit} className="delete-form">
          <label htmlFor="fetch-key-input"> Key: </label>
          <input id="fetch-key-input" type="text" value={key} onChange={(e) => setKey(e.target.value)}></input>
          <div className="button-group">
            <button type="button" className="secondary-button" onClick={clearForm}> Clear </button>
            <button type="submit" className="primary-button"> Delete </button>    
          </div>
        </form>
      </div>
      
    );
  }