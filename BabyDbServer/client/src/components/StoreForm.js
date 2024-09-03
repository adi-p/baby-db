import { forwardRef, useState } from 'react'
import { CONFIG } from "../Config";

const StoreForm = forwardRef((_, ref) => {
    const [key, setKey] = useState("");
    const [value, setValue] = useState("");
  
    const postStore = async (key, value) => {
      console.log(`Posting Key: ${key} and Value: ${value}`);
  
      try {
        let response = await fetch(`${CONFIG.HOST}/values`, {
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
      <div className="form-section">
        <form onSubmit={handleStoreSubmit} className="store-form">
          <label htmlFor="store-key-input"> Key: </label>
          <input ref={ref}  id="store-key-input" type="text" value={key} onChange={(e) => setKey(e.target.value)}></input>
          <label htmlFor="store-value-input"> Value: </label>
          <input id="store-value-input" type="text" value={value} onChange={(e) => setValue(e.target.value)}></input>
          <div className="button-group">
            <button type="button" className="secondary-button" onClick={clearForm}> Clear </button>
            <button type="submit" className="primary-button"> Store </button>
          </div>
        </form>
      </div>
    );
  });

  export default StoreForm;